using Discord;
using Magus.Data;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using Magus.DataBuild.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magus.DataBuilder.Extensions
{
    public static class PatchNoteExtensions
    {
        private static readonly string _patchUrlBase = "https://www.dota2.com/patches/";

        public static IEnumerable<GeneralPatchNoteEmbed> GetGeneralPatchNoteEmbeds(this PatchNote patch, Dictionary<string, string[]> languageMap)
        {
            var generalPatchEmbed = new Data.Models.Embeds.Embed()
            {
                Title        = $"Patch {patch.PatchName} - General changes",
                Description  = CreateFormattedDescription(patch.GenericNotes),
                Url          = _patchUrlBase + patch.PatchName,
                ColorRaw     = Color.DarkRed,
                Timestamp    = DateTimeOffset.FromUnixTimeSeconds((long)patch.Timestamp),
                ThumbnailUrl = $"https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/footer_logo.png",
                Footer       = new() { Text = "Patch " + patch.PatchName },
            };
            generalPatchEmbed.Fields = new List<Field>() { new() {
                    Name             = "Full patch notes",
                    Value            = $"[Click here for full patch notes]({generalPatchEmbed.Url})" +
                    $"\nAdditionally, use the command `/patch <hero|item> <name>` to view the most recent changes for a specifc hero or item "
                }};

            var generalPatchNotesList = new List<GeneralPatchNoteEmbed>();
            foreach (var locale in languageMap[patch.Language])
            {
                generalPatchNotesList.Add(new()
                {
                    Id          = MakeId(patch.Timestamp.ToString(), 0, 0, locale), //Temp custom id
                    Locale      = locale,
                    Embed       = generalPatchEmbed,
                    PatchNumber = patch.PatchName,
                });
            }
            return generalPatchNotesList;
        }

        public static IEnumerable<HeroPatchNoteEmbed> GetHeroPatchNoteEmbeds(this PatchNote patch, IEnumerable<HeroInfo> heroes, IEnumerable<AbilityInfo> abilities, Dictionary<string, string[]> languageMap)
        {
            var heroPatchNotesList = new List<HeroPatchNoteEmbed>();
            foreach (var hero in patch.HeroesNotes)
            {
                var heroInfo = heroes.Where(x => x.InternalName == hero.InternalName).First();

                var fields = new List<Field>();
                //if (hero.GeneralNotes.Count > 0)
                //{
                //    fields.Add(new() { Name = "General:", Value = CreateFormattedDescription(hero.GeneralNotes) });
                //}

                foreach (var abilityNote in hero.AbilityNotes)
                {
                    // Need to get ability info from gamefiles, as removed abilities don't exist here
                    //var abilityInfo = abilities.Where(x => x.InternalName == abilityNote.InternalName).First();
                    fields.Add(new() { Name = $"{abilityNote.InternalName}:", Value = CreateFormattedDescription(abilityNote.Notes) });
                }

                if (hero.TalentNotes.Count > 0)
                {
                    fields.Add(new() { Name = "Talents:", Value = CreateFormattedDescription(hero.TalentNotes) });
                }

                var heroPatchNoteEmbed = new Data.Models.Embeds.Embed()
                {
                    Title        = $"{heroInfo.LocalName} - changes {patch.PatchName}",
                    Description  = CreateFormattedDescription(hero.GeneralNotes),
                    Url          = _patchUrlBase + patch.PatchName,
                    ColorRaw     = Color.DarkOrange,
                    Timestamp    = DateTimeOffset.FromUnixTimeSeconds((long)patch.Timestamp),
                    ThumbnailUrl = $"https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/heroes/{hero.InternalName.Substring(14)}.png", // Store this in a hero object?
                    Footer       = new() { Text = "Patch " + patch.PatchName},
                    Fields       = fields,
                };
                foreach (var locale in languageMap[patch.Language])
                {
                    heroPatchNotesList.Add(new()
                    {
                        Id           = MakeId(patch.Timestamp.ToString(), 1, (int)heroInfo.Id, locale), //Temp custom id
                        Locale       = locale,
                        EntityId     = (int)heroInfo.Id,
                        LocalName    = heroInfo.LocalName,
                        InternalName = hero.InternalName,
                        Embed        = heroPatchNoteEmbed,
                        PatchNumber  = patch.PatchName,
                    });
                }
            }
            return heroPatchNotesList;
        }

        public static IEnumerable<ItemPatchNoteEmbed> GetItemPatchNoteEmbeds(this PatchNote patch, IEnumerable<ItemInfo> items, Dictionary<string, string[]> languageMap)
        {
            var itemPatchNotesList = new List<ItemPatchNoteEmbed>();

            foreach (var item in patch.ItemNotes.Concat(patch.NeutralItemNotes))
            {
                var itemInfo = items.Where(x => x.InternalName == item.InternalName).First();

                var itemPatchNoteEmbed = new Data.Models.Embeds.Embed()
                {
                    Title        = $"{itemInfo.LocalName} - changes {patch.PatchName}",
                    Description  = CreateFormattedDescription(item.Notes),
                    Url          = _patchUrlBase + patch.PatchName,
                    ColorRaw     = Color.DarkBlue,
                    Timestamp    = DateTimeOffset.FromUnixTimeSeconds((long)patch.Timestamp),
                    ThumbnailUrl = $"https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/items/{item.InternalName.Substring(5)}.png",
                    Footer       = new() { Text = "Patch " + patch.PatchName}
                };
                foreach (var locale in languageMap[patch.Language])
                {
                    itemPatchNotesList.Add(new()
                    {
                        Id           = MakeId(patch.Timestamp.ToString(), 2, (int)itemInfo.Id, locale), //Temp custom id
                        Locale       = locale,
                        EntityId     = (int)itemInfo.Id,
                        LocalName    = itemInfo.LocalName,
                        InternalName = itemInfo.InternalName,
                        Embed        = itemPatchNoteEmbed,
                        PatchNumber  = patch.PatchName,
                    });
                }
            }
            return itemPatchNotesList;
        }

        private static string CreateFormattedDescription(IList<PatchNote.Note> notes)
        {
            var description = string.Empty;
            foreach (var note in notes)
            {
                var tab = "• ";
                if (note.Indent > 0)
                {
                    tab = String.Concat(Enumerable.Repeat(Emotes.Spacer.ToString(), note.Indent)) + "◦ ";
                }
                description += tab + note.Value + "\n";
            }
            return description.ReplaceLocalFormatting();
        }

        private static ulong MakeId(string timestamp, int type, int id, string locale)
        {
            var localeInt = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(x=>x.Name == locale).First().LCID;
            return Convert.ToUInt64(String.Format("{0}{1}{2}{3}", timestamp, type, id.ToString().PadLeft(5, '0'), localeInt));
        }
    }
}
