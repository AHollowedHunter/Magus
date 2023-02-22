using Magus.Data;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ValveKeyValue;

namespace Magus.DataBuilder
{
    public class PatchNoteUpdater
    {
        private readonly IAsyncDataService _db;
        private readonly IConfiguration _config;
        private readonly ILogger<PatchNoteUpdater> _logger;
        private readonly HttpClient _httpClient;
        private readonly KVSerializer _kvSerializer;

        private readonly string _sourceDefaultLanguage;
        private readonly Dictionary<string, string[]> _sourceLocaleMappings; // Switch to config class
        private readonly Dictionary<(string Language, string Key), string> _patchNoteValues; // this should be its own class with methods
        private readonly Dictionary<(string Language, string Key), string> _dotaValues; // this should be its own class with methods
        private readonly Dictionary<(string Language, string Key), string> _abilityValues;
        private readonly List<PatchNote> _patchNotes;

        public PatchNoteUpdater(IAsyncDataService db, IConfiguration config, ILogger<PatchNoteUpdater> logger, HttpClient httpClient)
        {
            _db         = db;
            _config     = config;
            _logger     = logger;
            _httpClient = httpClient;

            _kvSerializer          = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            _sourceDefaultLanguage = _config.GetSection("Localisation").GetValue("DefaultLanguage", "english");
            _sourceLocaleMappings  = _config.GetSection("Localisation").GetSection("SourceLocaleMappings").Get<Dictionary<string, string[]>>();
            _patchNoteValues       = new();
            _dotaValues            = new();
            _patchNotes            = new();
            _abilityValues         = new();
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
            _logger.LogInformation("Time Taken: {0:0.#}s", timeTaken);
        }

        private async Task SetPatchNoteValues()
        {
            _logger.LogInformation("Setting Patch Note values");
            _patchNoteValues.Clear();
            _dotaValues.Clear();
            _abilityValues.Clear();
            foreach (var language in _sourceLocaleMappings)
            {
                _logger.LogDebug("Processing values for {0}", language.Key);
                var localePatchNotes = await _kvSerializer.GetKVObjectFromUri(Dota2GameFiles.Localization.GetPatchNotes(language.Key), _httpClient);
                foreach (var note in localePatchNotes)
                    _patchNoteValues.TryAdd((language.Key, note.Name), CleanLocaleValue(note.Value.ToString() ?? ""));

                var dota = await _kvSerializer.GetKVObjectFromUri(Dota2GameFiles.Localization.GetDota(language.Key), _httpClient);
                foreach (var note in dota.Children.First(x => x.Name == "Tokens"))
                    _dotaValues.TryAdd((language.Key, note.Name.ToLower()), CleanSimple(note.Value.ToString() ?? ""));

                var abilities = await _kvSerializer.GetKVObjectFromUri(Dota2GameFiles.Localization.GetAbilities(language.Key), _httpClient);
                foreach (var note in abilities.Children.First(x => x.Name == "Tokens"))
                    _abilityValues.TryAdd((language.Key, note.Name.ToLower()), note.Value.ToString() ?? "");
            }

            _logger.LogInformation("Finished setting Patch Note values");
        }

        private async Task SetPatchNotes()
        {
            _logger.LogInformation("Setting Patch Notes");
            var patchManifest = await _kvSerializer.GetKVObjectFromUri(Dota2GameFiles.PatchNotes, _httpClient);

            _patchNotes.Clear();

            foreach (var patch in patchManifest.Children)
            {
                foreach (var language in _sourceLocaleMappings.Keys)
                {
                    _patchNotes.Add(CreatePatchNote(language, patch));
                }
            }
            _logger.LogInformation("Finished setting Patch Notes");
        }

        private PatchNote CreatePatchNote(string language, KVObject patch)
        {
            var patchNote = new PatchNote();
            // Checks
            if (patch.Children.Where(x => x.Name == "items").First().Any(x => !x.Name.Contains("item_")))
            {
                _logger.LogWarning(patch["patch_name"] + "Contains items issue");
            }
            // End Checks

            patchNote.PatchName = patch.Children.First(x => x.Name == "patch_name").Value.ToString()!.Replace("patch ", "");
            patchNote.Timestamp = GetPatchTimestamp(patch);
            patchNote.Language  = language;
            patchNote.Website   = patch.Children.FirstOrDefault(x => x.Name == "website")?.Value.ToString();

            foreach (var genericNote in patch.Children.First(x => x.Name == "generic"))
            {
                patchNote.GenericNotes.Add(MakeNote(genericNote, language));
            }

            foreach (var item in patch.Children.First(x => x.Name == "items").Children)
            {
                var notes = new List<PatchNote.Note>();
                foreach (var note in item.Children.Where(x => x.Name == "note"))
                {
                    notes.Add(MakeNote(note, language));
                }

                var existingNote = patchNote.ItemNotes.FirstOrDefault(x => x.InternalName == item.Name);
                if (existingNote != null)
                {
                    patchNote.ItemNotes.Remove(existingNote);
                    var combinedNotes = new List<PatchNote.Note>();
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
                        Title        = item.Children.FirstOrDefault(x => x.Name == "Title")?.Value.ToString(),
                        Notes        = notes,
                    });
                }
            }

            foreach (var item in patch.Children.First(x => x.Name == "items_neutral").Children)
            {
                var notes = new List<PatchNote.Note>();
                foreach (var note in item.Children.Where(x => x.Name == "note"))
                {
                    notes.Add(MakeNote(note, language));
                }
                patchNote.NeutralItemNotes.Add(new()
                {
                    InternalName = item.Name,
                    Title        = item.Children.FirstOrDefault(x => x.Name == "Title")?.Value.ToString(),
                    Notes        = notes,
                });
            }

            foreach (var hero in patch.Children.First(x => x.Name == "heroes").Children)
            {
                var generalNotes = new List<PatchNote.Note>();
                var abilityNotes = new List<PatchNote.AbilityNotes>();
                var talentNotes  = new List<PatchNote.Note>();
                foreach (var general in hero.Children.Where(x => x.Name == "default"))
                {
                    foreach (var note in general.Children)
                    {
                        generalNotes.Add(MakeNote(note, language));
                    }
                }
                foreach (var ability in hero.Children.Where(x => x.Name.StartsWith(hero.Name.Substring(14 /* because heroname includes full "npc_dota_hero_" */))))
                {
                    var notes = new List<PatchNote.Note>();
                    foreach (var note in ability.Children.Where(x => x.Name == "note"))
                    {
                        notes.Add(MakeNote(note, language));
                    }
                    abilityNotes.Add(new()
                    {
                        InternalName = ability.Name,
                        Notes        = notes,
                        Title        = ability.Children.FirstOrDefault(x => x.Name == "Title")?.Value.ToString()
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
                    TalentNotes  = talentNotes,
                });
            }

            foreach (var creep in patch.Children.First(x => x.Name == "neutral_creeps").Children)
            {
                var notes = new List<PatchNote.Note>();
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

            _logger.LogDebug("Processed patch note for {0,-5} in {1}", patchNote.PatchName, language);
            return patchNote;
        }

        private ulong GetPatchTimestamp(KVObject patch)
            => (ulong)DateTimeOffset.Parse(patch.Children.First(x => x.Name == "patch_date").Value.ToString()!).ToUnixTimeSeconds();

        private async Task StorePatchNoteEmbeds()
        {
            _logger.LogInformation("Converting Patch Notes to Embed records");

            // Following will need tweaking to use collections representing localised entity data from Magus.Data.Models.Dota
            // For now, while hero, ability, item etc. data is not procesed, use existing ...Info stored

            EnsureIndexes();
            foreach (var localeMap in _sourceLocaleMappings)
                foreach (var locale in localeMap.Value)
                {
                    var generalPatchNotes = new List<GeneralPatchNoteEmbed>();
                    var heroPatchNotes    = new List<HeroPatchNoteEmbed>();
                    var itemPatchNotes    = new List<ItemPatchNoteEmbed>();
                    var heroInfo = await _db.GetRecords<HeroInfoEmbed>(locale);
                    var abilityInfo = await _db.GetRecords<AbilityInfoEmbed>(locale);
                    var itemInfo = await _db.GetRecords<ItemInfoEmbed>(locale);

                    foreach (var patch in _patchNotes.Where(x => x.Language == localeMap.Key))
                    {
                        _logger.LogDebug("Processing patch embeds {0,-5} in {1}", patch.PatchName, patch.Language);

                        generalPatchNotes.AddRange(patch.GetGeneralPatchNoteEmbeds(_sourceLocaleMappings));
                        heroPatchNotes.AddRange(patch.GetHeroPatchNoteEmbeds(heroInfo, _abilityValues, _sourceLocaleMappings));
                        itemPatchNotes.AddRange(patch.GetItemPatchNoteEmbeds(itemInfo, _sourceLocaleMappings));
                    }
                    _logger.LogInformation("Updating Patch Notes in Database for {0}", locale);
                    await _db.UpsertRecords(generalPatchNotes);
                    await _db.UpsertRecords(heroPatchNotes);
                    await _db.UpsertRecords(itemPatchNotes);
                }
        }

        private void EnsureIndexes()
        {
            _db.CreateCollection<GeneralPatchNoteEmbed>();
            _db.CreateCollection<HeroPatchNoteEmbed>();
            _db.CreateCollection<ItemPatchNoteEmbed>();
            _db.EnsureIndex<GeneralPatchNoteEmbed>(x => x.PatchNumber, caseSensitive: false);
            _db.EnsureIndex<HeroPatchNoteEmbed>(x => x.PatchNumber, caseSensitive: false);
            _db.EnsureIndex<HeroPatchNoteEmbed>(x => x.EntityId);
            _db.EnsureIndex<HeroPatchNoteEmbed>(x => x.Name, caseSensitive: false);
            _db.EnsureIndex<HeroPatchNoteEmbed>(x => x.Aliases!, caseSensitive: false);
            _db.EnsureIndex<HeroPatchNoteEmbed>(x => x.RealName!, caseSensitive: false);
            _db.EnsureIndex<ItemPatchNoteEmbed>(x => x.PatchNumber, caseSensitive: false);
            _db.EnsureIndex<ItemPatchNoteEmbed>(x => x.Name, caseSensitive: false);
            _db.EnsureIndex<ItemPatchNoteEmbed>(x => x.Aliases!, caseSensitive: false);
        }

        private PatchNote.Note MakeNote(KVObject kvObject, string language)
        {
            if (kvObject.Children.Count() == 0)
            {
                return new() { Value = _patchNoteValues[(language, kvObject.Value.ToString()!)] };
            }
            else
            {
                var info = kvObject.Children.FirstOrDefault(x => x.Name == "info")?.Value.ToString();
                if (info != null)
                {
                    var valueKey = (language, info.Substring(1));
                    info = GetLanguageValueOrDefault(valueKey);
                }
                var noteKey = (language, kvObject.Children.First(x => x.Name == "note").Value.ToString()!.Substring(1));
                return new()
                {
                    Value  = GetLanguageValueOrDefault(noteKey)!,
                    Indent = Math.Abs(int.Parse(kvObject.Children.FirstOrDefault(x => x.Name == "indent")?.Value.ToString() ?? "0")),
                    Info   = info,
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
                _patchNoteValues.TryGetValue((_sourceDefaultLanguage, key.key), out var value);
                return value;
            }
        }

        private static string CleanSimple(string value)
        {
            var boldRegex    = new Regex(@"(?i)<[/]?\s*b\s*>");
            var italicsRegex = new Regex(@"(?i)<[/]?\s*i\s*>");
            var htmlTagRegex   = new Regex(@"(?i)<[/]?\s*[^>]*>");
            value            = value.Replace("<br>", "\n");
            value            = boldRegex.Replace(value, "**");
            value            = italicsRegex.Replace(value, "*");
            value              = htmlTagRegex.Replace(value, "");

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
            patchNote          = onlyBreak.Replace(patchNote, "\n");
            patchNote          = patchNote.Replace("<br>", "\n");
            patchNote          = patchNote.Replace("&nbsp;", "");
            patchNote          = patchNote.Replace("*", "\\*");
            patchNote          = tableRegex.Replace(patchNote, "");
            patchNote          = boldRegex.Replace(patchNote, "**");
            patchNote          = infoRegex.Replace(patchNote, "*");
            patchNote          = highlightRegex.Replace(patchNote, "__");
            patchNote          = htmlTagRegex.Replace(patchNote, "");

            return patchNote;
        }
    }
}
