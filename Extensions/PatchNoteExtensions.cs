﻿using Discord;
using Magus.Data;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using System.Text;
using System.Text.RegularExpressions;

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
                Description  = CreateFormattedDescription(patch.GenericNotes), // LIMIT IT
                Url          = _patchUrlBase + patch.PatchName,
                ColorRaw     = Color.DarkRed,
                Timestamp    = DateTimeOffset.FromUnixTimeSeconds((long)patch.Timestamp),
                ThumbnailUrl = $"https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/footer_logo.png",
                Footer       = new() { Text = "Patch " + patch.PatchName },
            };
            var fields = new List<Field>();

            if (!string.IsNullOrEmpty(patch.Website))
            {
                fields.Add(new()
                {
                    Name  = "Patch Website",
                    Value = $"[{patch.Website}](https://www.dota2.com/{patch.Website})",
                });
            }
            fields.Add(new()
            {
                Name  = "Full patch notes",
                Value = $"[Click here for full patch notes]({generalPatchEmbed.Url})" +
                $"\nAdditionally, use the command `/patch <hero|item> <name>` to view the most recent changes for a specifc hero or item ",
            });

            generalPatchEmbed.Fields = fields;

            var generalPatchNotesList = new List<GeneralPatchNoteEmbed>();
            foreach (var locale in languageMap[patch.Language])
            {
                generalPatchNotesList.Add(new()
                {
                    Id          = GetPatchNoteId(patch.PatchName,"General", locale), //Temp custom id
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
                        Id           = GetPatchNoteId(patch.PatchName, hero.InternalName, locale), //Temp custom id
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
                        Id           = GetPatchNoteId(patch.PatchName, item.InternalName, locale), //Temp custom id
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

        private static string CreateFormattedDescription(IList<PatchNote.Note> notes, int maxLength = 4096)
        {
            var description = string.Empty;
            string truncatedMessage = "***See website for full patchnote***";
            foreach (var note in notes)
            {
                var indent = notes.Any(x=> x.Indent == 0) ? note.Indent : note.Indent - 1; // Some set of notes are all indedented, so remove a level
                var tab = string.Empty;

                if (!Regex.Match(note.Value, @"^\s+$").Success)
                {
                    tab = GetTab(indent);
                }

                var valueToAdd = tab + note.Value + "\n";
                if (!string.IsNullOrEmpty(note.Info))
                {
                    valueToAdd += $"{GetTab(indent + 1)}{note.Info}\n";
                }

                if (description.Length + valueToAdd.Length + truncatedMessage.Length > maxLength)
                {
                    description += truncatedMessage;
                    break;
                }
                description += valueToAdd;
            }
            return description;
        }

        private static string GetTab(int indent)
        {
            if (indent > 0)
            {
                return String.Concat(Enumerable.Repeat(Emotes.Spacer.ToString(), indent)) + "◦ ";
            }
            return "• ";
        }

        private static ulong GetPatchNoteId(string patchname, string entityName, string locale)
        {
            var str = $"{patchname}_{entityName}_{locale}";
            var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            var id = BitConverter.ToUInt64(hash);
            return id;
        }
    }
}
