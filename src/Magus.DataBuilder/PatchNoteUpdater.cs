using Magus.Common.Dota.Models;
using Magus.Common.Options;
using Magus.Data.Enums;
using Magus.Data.Models.Dota;
using Magus.Data.Services;
using Meilisearch;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ValveKeyValue;

namespace Magus.DataBuilder;

public sealed class PatchNoteUpdater
{
    private readonly LocalisationOptions _localisationOptions;
    private readonly ILogger<PatchNoteUpdater> _logger;
    private readonly MeilisearchService _meilisearchService;
    private readonly KVSerializer _kvSerializer;

    private readonly Dictionary<(string Language, string Key), string> _patchNoteValues; // this should be its own class with methods
    private readonly Dictionary<(string Language, string Key), string> _dotaValues; // this should be its own class with methods
    private readonly Dictionary<(string Language, string Key), string> _abilityValues;
    private readonly List<PatchNotes> _patchNotes;

    public PatchNoteUpdater(IOptions<LocalisationOptions> localisationOptions, ILogger<PatchNoteUpdater> logger, MeilisearchService meilisearchService)
    {
        //_db = db;
        _localisationOptions = localisationOptions.Value;
        _logger = logger;
        _meilisearchService = meilisearchService;

        _kvSerializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        _patchNoteValues = new();
        _dotaValues = new();
        _patchNotes = new();
        _abilityValues = new();
    }

    public async Task Update()
    {
        _logger.LogInformation("Starting Patch Note Update");
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        await SetPatchNoteValues();
        await SetPatchNotes();

        await StorePatchNoteEmbeds();
        stopwatch.Stop();
        var timeTaken = stopwatch.Elapsed.TotalSeconds;
        _logger.LogInformation("Finished Patch Note Update");
        _logger.LogInformation("Time Taken: {time:0.#}s", timeTaken);
    }

    private async Task SetPatchNoteValues()
    {
        _logger.LogInformation("Setting Patch Note values");
        _patchNoteValues.Clear();
        _dotaValues.Clear();
        _abilityValues.Clear();
        foreach (var language in _localisationOptions.SourceLocaleMappings)
        {
            _logger.LogDebug("Processing values for {key}", language.Key);
            var localePatchNotes = await _kvSerializer.GetKVObjectFromLocalUri(Dota2GameFiles.Localization.GetPatchNotes(language.Key));
            foreach (var note in localePatchNotes)
                _patchNoteValues.TryAdd((language.Key, note.Name), CleanLocaleValue(note.Value.ToString() ?? ""));

            var dota = await _kvSerializer.GetKVObjectFromLocalUri(Dota2GameFiles.Localization.GetDota(language.Key));
            foreach (var note in dota.Children.First(x => x.Name == "Tokens"))
                _dotaValues.TryAdd((language.Key, note.Name.ToLower()), CleanSimple(note.Value.ToString() ?? ""));

            var abilities = await _kvSerializer.GetKVObjectFromLocalUri(Dota2GameFiles.Localization.GetAbilities(language.Key));
            foreach (var note in abilities.Children.First(x => x.Name == "Tokens"))
                _abilityValues.TryAdd((language.Key, note.Name.ToLower()), note.Value.ToString() ?? "");
        }

        _logger.LogInformation("Finished setting Patch Note values");
    }

    private async Task SetPatchNotes()
    {
        _logger.LogInformation("Setting Patch Notes");
        var patchManifest = await _kvSerializer.GetKVObjectFromLocalUri(Dota2GameFiles.PatchNotes);

        _patchNotes.Clear();

        foreach (var patch in patchManifest.Children)
        {
            foreach (var language in _localisationOptions.SourceLocaleMappings.Keys)
            {
                _patchNotes.Add(CreatePatchNote(language, patch));
            }
        }
        _logger.LogInformation("Finished setting Patch Notes");
    }

    private PatchNotes CreatePatchNote(string language, KVObject patch)
    {
        var patchNote = new PatchNotes();
        // Checks
        if (patch.Children.Where(x => x.Name == "items").First().Any(x => !x.Name.Contains("item_")))
        {
            _logger.LogWarning("{patch} Contains items issue", patch["patch_name"]);
        }
        // End Checks

        patchNote.PatchName = patch.Children.First(x => x.Name == "patch_name").Value.ToString()!.Replace("patch ", "");
        patchNote.Timestamp = GetPatchTimestamp(patch);
        patchNote.Language = language;
        patchNote.Website = patch.Children.FirstOrDefault(x => x.Name == "website")?.Value.ToString();

        foreach (var genericNote in patch.Children.First(x => x.Name == "generic"))
        {
            patchNote.GenericNotes.Add(MakeNote(genericNote, language));
        }

        foreach (var item in patch.Children.First(x => x.Name == "items").Children)
        {
            var notes = new List<PatchNotes.Note>();
            foreach (var note in item.Children.Where(x => x.Name == "note"))
            {
                notes.Add(MakeNote(note, language));
            }

            var existingNote = patchNote.ItemNotes.FirstOrDefault(x => x.InternalName == item.Name);
            if (existingNote != null)
            {
                patchNote.ItemNotes.Remove(existingNote);
                var combinedNotes = new List<PatchNotes.Note>();
                combinedNotes.AddRange(existingNote.Notes);
                combinedNotes.AddRange(notes);
                existingNote.Notes = combinedNotes;
                patchNote.ItemNotes.Add(existingNote); //todo does this combine separate items? 
            }
            else
            {
                patchNote.ItemNotes.Add(new()
                {
                    InternalName = item.Name,
                    Title = item.Children.FirstOrDefault(x => x.Name == "Title")?.Value.ToString(),
                    Notes = notes,
                });
            }
        }

        foreach (var item in patch.Children.First(x => x.Name == "items_neutral").Children)
        {
            var notes = new List<PatchNotes.Note>();
            foreach (var note in item.Children.Where(x => x.Name == "note"))
            {
                notes.Add(MakeNote(note, language));
            }
            patchNote.NeutralItemNotes.Add(new()
            {
                InternalName = item.Name,
                Title = item.Children.FirstOrDefault(x => x.Name == "Title")?.Value.ToString(),
                Notes = notes,
            });
        }

        foreach (var hero in patch.Children.First(x => x.Name == "heroes").Children)
        {
            var generalNotes = new List<PatchNotes.Note>();
            var abilityNotes = new List<PatchNotes.AbilityNotes>();
            var talentNotes  = new List<PatchNotes.Note>();
            foreach (var general in hero.Children.Where(x => x.Name == "default"))
            {
                foreach (var note in general.Children)
                {
                    generalNotes.Add(MakeNote(note, language));
                }
            }
            foreach (var ability in hero.Children.Where(x => x.Name.StartsWith(hero.Name[14..])))/* because heroname includes full "npc_dota_hero_" */
            {
                var notes = new List<PatchNotes.Note>();
                foreach (var note in ability.Children.Where(x => x.Name == "note"))
                {
                    notes.Add(MakeNote(note, language));
                }
                abilityNotes.Add(new()
                {
                    InternalName = ability.Name,
                    Notes = notes,
                    Title = ability.Children.FirstOrDefault(x => x.Name == "Title")?.Value.ToString()
                });
            }
            foreach (var talent in hero.Children.Where(x => x.Name == "talent"))
            {
                foreach (var note in talent.Children)
                {
                    talentNotes.Add(MakeNote(note, language));
                }
            }

            patchNote.HeroesNotes.Add(new()
            {
                InternalName = hero.Name,
                GeneralNotes = generalNotes,
                AbilityNotes = abilityNotes,
                TalentNotes = talentNotes,
            });
        }

        foreach (var creep in patch.Children.First(x => x.Name == "neutral_creeps").Children)
        {
            var notes = new List<PatchNotes.Note>();
            foreach (var note in creep.Children.Where(x => x.Name == "note"))
            {
                notes.Add(MakeNote(note, language));
            }
            patchNote.NeutralCreepNotes.Add(new()
            {
                InternalName = creep.Name,
                Title = creep.Children.FirstOrDefault(x => x.Name == "Title")?.Value.ToString(),
                Notes = notes,
            });
        }

        _logger.LogDebug("Processed patch note for {name,-5} in {lang}", patchNote.PatchName, language);
        return patchNote;
    }

    private static long GetPatchTimestamp(KVObject patch)
        => DateTimeOffset.Parse(patch.Children.First(x => x.Name == "patch_date").Value.ToString()!).ToUnixTimeSeconds();

    private async Task StorePatchNoteEmbeds()
    {
        _logger.LogInformation("Converting Patch Notes to Embed records");

        // Following will need tweaking to use collections representing localised entity data from Magus.Data.Models.Dota
        // For now, while hero, ability, item etc. data is not procesed, use existing ...Info stored

        try { await _meilisearchService.DeleteIndexAsync(nameof(PatchNote)); } catch { } // HACK for testing. Will use a swap index later.
        string[] filterableAttributes =
                [
                    nameof(PatchNote.PatchNumber),
                    nameof(PatchNote.PatchNoteType),
                    nameof(PatchNote.EntityType),
                    nameof(PatchNote.Locale)
                ];
        string[] searchableAttributes =
                [
                    nameof(PatchNote.PatchNumber),
                    nameof(PatchNote.Timestamp),
                    nameof(PatchNote.InternalName),
                    nameof(PatchNote.EntityId),
                    nameof(PatchNote.Locale)
                ];
        string[] sortableAttributes =
                [
                    nameof(PatchNote.PatchNumber),
                    nameof(PatchNote.Timestamp),
                ];
        Settings settings = new()
        {
            FilterableAttributes = filterableAttributes,
            SearchableAttributes = searchableAttributes,
            SortableAttributes = sortableAttributes
        };
        await _meilisearchService.CreateIndexAsync(nameof(PatchNote), nameof(PatchNote.UniqueId), settings);

        foreach (var localeMap in _localisationOptions.SourceLocaleMappings)
            foreach (var locale in localeMap.Value)
            {
                List<PatchNote> patchNotes = [];
                var heroInfo          = await _meilisearchService.GetAllEntityInfoAsync(EntityType.Hero, locale);
                //var abilityInfo       = await _db.GetRecords<AbilityInfoEmbed>(locale);
                var itemInfo          = await _meilisearchService.GetAllEntityInfoAsync(EntityType.Item, locale);

                foreach (var patch in _patchNotes.Where(x => x.Language == localeMap.Key))
                {
                    _logger.LogDebug("Processing patch embeds {name,-5} in {lang}", patch.PatchName, patch.Language);

                    patchNotes.Add(patch.GetGeneralPatchNoteEmbeds(locale));
                    patchNotes.AddRange(patch.GetHeroPatchNoteEmbeds(heroInfo, _abilityValues, locale));
                    patchNotes.AddRange(patch.GetItemPatchNoteEmbeds(itemInfo, locale));
                }
                _logger.LogInformation("Updating Patch Notes in Database for {locale}", locale);
                await _meilisearchService.AddDocumentsAsync(patchNotes);
            }
    }

    private PatchNotes.Note MakeNote(KVObject kvObject, string language)
    {
        if (!kvObject.Children.Any())
        {
            return new() { Value = _patchNoteValues[(language, kvObject.Value.ToString()!)] };
        }
        else
        {
            var info = kvObject.Children.FirstOrDefault(x => x.Name == "info")?.Value.ToString();
            if (info != null)
            {
                var valueKey = (language, info[1..]);
                info = GetLanguageValueOrDefault(valueKey);
            }
            var noteKey = (language, kvObject.Children.First(x => x.Name == "note").Value.ToString() ![1 ..]);
            return new()
            {
                Value = GetLanguageValueOrDefault(noteKey)!,
                Indent = Math.Abs(int.Parse(kvObject.Children.FirstOrDefault(x => x.Name == "indent")?.Value.ToString() ?? "0")),
                Info = info,
            };
        }
    }

    private string? GetLanguageValueOrDefault((string language, string key) key)
    {
        if (_patchNoteValues.ContainsKey(key))
        {
            return _patchNoteValues[key];
        }
        else
        {
            _patchNoteValues.TryGetValue((_localisationOptions.DefaultLanguage, key.key), out var value);
            return value;
        }
    }

    private static string CleanSimple(string value)
    {
        var boldRegex    = new Regex(@"(?i)<[/]?\s*b\s*>");
        var italicsRegex = new Regex(@"(?i)<[/]?\s*i\s*>");
        var htmlTagRegex = new Regex(@"(?i)<[/]?\s*[^>]*>");

        value = value.Replace("<br>", "\n");
        value = boldRegex.Replace(value, "**");
        value = italicsRegex.Replace(value, "*");
        value = htmlTagRegex.Replace(value, "");

        return value;
    }

    private static string CleanLocaleValue(string patchNote)
    {
        var onlyBreak      = new Regex(@"^\s*<br>\s*$");
        var tableRegex     = new Regex(@"<table>(.|\n)*<\/table>");
        var boldRegex      = new Regex(@"(?i)<[/]?\s*b\s*/?>");
        var infoRegex      = new Regex(@"(?i)<[/]?\s*info\s*/?>");
        var highlightRegex = new Regex(@"(?i)<[/]?[\s.]*(class=""(New|Reworked)"")?[^>]*>");
        var htmlTagRegex   = new Regex(@"(?i)<[/]?\s*[^>]*>");

        patchNote = onlyBreak.Replace(patchNote, "\n");
        patchNote = patchNote.Replace("<br>", "\n");
        patchNote = patchNote.Replace("&nbsp;", "");
        patchNote = patchNote.Replace("*", "\\*");
        patchNote = tableRegex.Replace(patchNote, "");
        patchNote = boldRegex.Replace(patchNote, "**");
        patchNote = infoRegex.Replace(patchNote, "*");
        patchNote = highlightRegex.Replace(patchNote, "__");
        patchNote = htmlTagRegex.Replace(patchNote, "");

        return patchNote;
    }
}
