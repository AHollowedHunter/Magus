using Magus.Data;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using Magus.DataBuilder.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;
using ValveKeyValue;

namespace Magus.DataBuilder
{
    public class PatchNoteUpdater
    {
        private readonly IDatabaseService _db;
        private readonly IConfiguration _config;
        private readonly ILogger<PatchNoteUpdater> _logger;
        private readonly HttpClient _httpClient;
        private readonly KVSerializer _kvSerializer;

        private readonly string _sourceDefaultLanguage;
        private readonly Dictionary<string, string[]> _sourceLocaleMappings; // Switch to config class
        private readonly Dictionary<(string Language, string Key), string> _patchNoteValues; // this should be its own class with methods
        private readonly List<Patch> _patches;
        private readonly List<PatchNote> _patchNotes;

        public PatchNoteUpdater(IDatabaseService db, IConfiguration config, ILogger<PatchNoteUpdater> logger, HttpClient httpClient)
        {
            _db         = db;
            _config     = config;
            _logger     = logger;
            _httpClient = httpClient;

            _kvSerializer          = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            _sourceDefaultLanguage = _config.GetSection("Localisation").GetValue("DefaultLanguage", "english");
            _sourceLocaleMappings  = _config.GetSection("Localisation").GetSection("SourceLocaleMappings").Get<Dictionary<string, string[]>>();
            _patchNoteValues       = new();
            _patches               = new();
            _patchNotes            = new();
        }

        public async Task Update()
        {
            _logger.LogInformation("Starting Patch Note Update");
            var startTime = DateTimeOffset.Now;
            await SetPatchNoteValues();
            await SetPatchNotes();

            StorePatchNoteEmbeds();
            var endTime   = DateTimeOffset.Now;
            var timeTaken = endTime-startTime;
            _logger.LogInformation("Finished Patch Note Update");
            _logger.LogInformation("Time Taken: {0:0.#}s", timeTaken.TotalSeconds);
        }

        private async Task SetPatchNoteValues()
        {
            _logger.LogInformation("Setting Patch Note values");
            _patchNoteValues.Clear();
            foreach (var language in _sourceLocaleMappings)
            {
                _logger.LogDebug("Processing values for {0}", language.Key);
                var localePatchNotes = await GetKVObjectFromUri(Dota2GameFiles.Localization.GetPatchNotes(language.Key));
                foreach (var note in localePatchNotes)
                    _patchNoteValues.Add((language.Key, note.Name), CleanLocaleValue(note.Value.ToString() ?? ""));
            }
            _logger.LogInformation("Finished setting Patch Note values");
        }

        private async Task SetPatchNotes()
        {
            _logger.LogInformation("Setting Patch Notes");
            var patchManifest = await GetKVObjectFromUri(Dota2GameFiles.PatchNotes);

            _patches.Clear();
            _patchNotes.Clear();

            foreach (var patch in patchManifest.Children)
            {
                _patches.Add(CreatePatchInfo(patch));
                foreach (var language in _sourceLocaleMappings.Keys)
                {
                    _patchNotes.Add(CreatePatchNote(language, patch));
                }
            }
            _logger.LogInformation("Finished setting Patch Notes");
        }

        private Patch CreatePatchInfo(KVObject patch)
            => new()
            {
                Id             = GetPatchTimestamp(patch),
                PatchNumber    = patch.Children.First(x => x.Name == "patch_name").Value.ToString()!.Replace("patch ", ""),
                Timestamp      = GetPatchTimestamp(patch),
            };

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

        private void StorePatchNoteEmbeds()
        {
            _logger.LogInformation("Converting Patch Notes to Embed records");
            var generalPatchNotes = new List<GeneralPatchNoteEmbed>();
            var heroPatchNotes    = new List<HeroPatchNoteEmbed>();
            var itemPatchNotes    = new List<ItemPatchNoteEmbed>();

            // Following will need tweaking to use collections representing localised entity data from Magus.Data.Models.Dota
            // For now, while hero, ability, item etc. data is not procesed, use existing ...Info stored
            var heroInfo    = _db.GetRecords<HeroInfo>();
            var abilityInfo = _db.GetRecords<AbilityInfo>();
            var itemInfo    = _db.GetRecords<ItemInfo>();

            foreach (var patch in _patchNotes)
            {
                _logger.LogDebug("Processing patch embeds {0,-5} in {1}", patch.PatchName, patch.Language);
                generalPatchNotes.AddRange(patch.GetGeneralPatchNoteEmbeds(_sourceLocaleMappings));
                heroPatchNotes.AddRange(patch.GetHeroPatchNoteEmbeds(heroInfo, abilityInfo, _sourceLocaleMappings));
                itemPatchNotes.AddRange(patch.GetItemPatchNoteEmbeds(itemInfo, _sourceLocaleMappings));
            }
            _logger.LogInformation("Updating Patch Notes in Database");
            _db.DeleteCollection<Patch>(); //Patch uses random ID, so wipe collection and re-add. TODO: change this to use fixed id
            _db.InsertRecords(_patches);
            _db.UpsertRecords(generalPatchNotes);
            _db.UpsertRecords(heroPatchNotes);
            _db.UpsertRecords(itemPatchNotes);
            EnsureIndexes();
        }

        private void EnsureIndexes()
        {
            _db.EnsureIndex<Patch>("PatchNumber");
            _db.EnsureIndex<GeneralPatchNoteEmbed>("PatchNumber");
            _db.EnsureIndex<HeroPatchNoteEmbed>("EntityId");
            _db.EnsureIndex<HeroPatchNoteEmbed>("PatchNumber");
            _db.EnsureIndex<ItemPatchNoteEmbed>("EntityId");
            _db.EnsureIndex<ItemPatchNoteEmbed>("PatchNumber");
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

        private async Task<KVObject> GetKVObjectFromUri(string uri)
        {
            var rawString = await _httpClient.GetStringAsync(uri);
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(rawString));
            return _kvSerializer.Deserialize(stream, new KVSerializerOptions() { HasEscapeSequences = true });
        }
    }
}
