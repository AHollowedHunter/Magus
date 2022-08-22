using Magus.Data;
using Magus.Data.Models.Dota;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ValveKeyValue;

namespace Magus.DataBuilder
{
    public class PatchNoteUpdater
    {
        private readonly IDatabaseService _db;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly KVSerializer _kvSerializer;

        private readonly Dictionary<string, string[]> _sourceLocaleMappings;
        private readonly Dictionary<(string Locale, string Key), string> _patchNoteValues;
        private readonly List<PatchNote> _patchNotes;

        public PatchNoteUpdater(IDatabaseService db, IConfiguration config, HttpClient httpClient)
        {
            _db = db;
            _config = config;
            _httpClient = httpClient;

            _kvSerializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            _sourceLocaleMappings = _config.GetSection("Localisation").GetSection("SourceLocaleMappings").Get<Dictionary<string, string[]>>();
            _patchNoteValues = new();
            _patchNotes = new();
        }

        public async Task Update()
        {
            await SetPatchNoteValues();
            await SetPatchNotes();


        }

        private async Task SetPatchNoteValues()
        {
            _patchNoteValues.Clear();
            foreach (var language in _sourceLocaleMappings)
            {
                var localePatchNotes = await GetKVObjectFromUri(Dota2GameFiles.Localization.GetPatchNotes(language.Key));
                foreach (var note in localePatchNotes)
                    foreach (var langCode in language.Value)
                        _patchNoteValues.Add((langCode, note.Name), CleanLocaleValue(note.Value.ToString() ?? ""));
            }
        }

        private async Task SetPatchNotes()
        {
            var patchManifest = await GetKVObjectFromUri(Dota2GameFiles.PatchNotes);

            _patchNotes.Clear();

            foreach (var language in _sourceLocaleMappings.Keys)
            {
                foreach (var patch in patchManifest.Children)
                {
                    _patchNotes.Add(CreatePatchNote(language, patch));
                }
            }
        }

        private PatchNote CreatePatchNote(string language, KVObject patch)
        {
            var patchNote = new PatchNote();
            // Checks
            if (patch.Children.Where(x => x.Name == "items").First().Any(x => !x.Name.Contains("item_")))
            {
                Console.WriteLine(patch["patch_name"] + "Contains items issue");
            }
            // End Checks

            patchNote.PatchName = patch.Children.First(x => x.Name == "patch_name").Value.ToString()!.Replace("patch ", "");
            var timestampString = patch.Children.First(x => x.Name == "patch_date").Value.ToString()!;
            ulong timestamp = (ulong)DateTimeOffset.Parse(timestampString).ToUnixTimeSeconds();
            patchNote.Timestamp = timestamp;
            patchNote.Language = language;

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
                patchNote.ItemNotes.Add(new()
                {
                    Name = item.Name,
                    Title = item.Children.FirstOrDefault(x => x.Name == "Title")?.Value.ToString(),
                    Notes = notes,
                });
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
                    Name = item.Name,
                    Title = item.Children.FirstOrDefault(x => x.Name == "Title")?.Value.ToString(),
                    Notes = notes,
                });
            }

            foreach (var hero in patch.Children.First(x => x.Name == "heroes").Children)
            {
                var generalNotes = new List<PatchNote.Note>();
                var abilityNotes = new List<PatchNote.AbilityNotes>();
                var talentNotes = new List<PatchNote.Note>();
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
                        Name = ability.Name,
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
                    Name = hero.Name,
                    GeneralNotes = generalNotes,
                    AbilityNotes = abilityNotes,
                    TalentNotes = talentNotes,
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
                    Name = creep.Name,
                    Title = creep.Children.FirstOrDefault(x => x.Name == "Title")?.Value.ToString(),
                    Notes = notes,
                });
            }
            return patchNote;
        }

        private async Task CreatePatchNoteEmbeds()
        {
            var generalPatchNotes = new List<Data.Models.Embeds.GeneralPatchNote>();
            var heroPatchNotes = new List<Data.Models.Embeds.HeroPatchNote>();
            var itemPatchNotes = new List<Data.Models.Embeds.ItemPatchNote>();
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
                return new()
                {
                    Value = _patchNoteValues[(language, kvObject.Children.First(x => x.Name == "note").Value.ToString()!.Substring(1))],
                    Indent = Math.Abs(int.Parse(kvObject.Children.FirstOrDefault(x => x.Name == "indent")?.Value.ToString() ?? "0")),
                    Info = info != null ? _patchNoteValues[(language, info.Substring(1))] : info,
                };
            }
        }

        private static string CleanLocaleValue(string patchNote)
        {
            var tableRegex = new Regex(@"<table>(.|\n)*<\/table>");
            var boldRegex = new Regex(@"(?i)<[/]?\s*b\s*/?>");
            var infoRegex = new Regex(@"(?i)<[/]?\s*info\s*/?>");
            var highlightRegex = new Regex(@"(?i)<[/]?[\s.]*(class=""(New|Reworked)"")?[^>]*>");
            var htmlTagRegex = new Regex(@"(?i)<[/]?\s*[^>]*>");
            patchNote = patchNote.Replace("<br>", "\n");
            patchNote = patchNote.Replace("&nbsp;", "");
            patchNote = tableRegex.Replace(patchNote, "");
            patchNote = boldRegex.Replace(patchNote, "**");
            patchNote = infoRegex.Replace(patchNote, "*");
            patchNote = highlightRegex.Replace(patchNote, "__");
            patchNote = htmlTagRegex.Replace(patchNote, "");

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
