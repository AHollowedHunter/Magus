using Discord;
using Magus.Data;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using Magus.DataBuild.Models;

namespace Magus.DataBuilder
{
    public class PatchNoteUtils
    {
        public static Data.Models.Embeds.GeneralPatchNote GetGeneralPatchNote(Patch patchData, RawPatchNote patchNoteData)
        {
            var notes = new List<Note>();
            foreach (var generic in patchNoteData.generic ?? new List<RawPatchNote.Generic>())
            {
                if (generic.note == "<br>")
                {
                    continue;
                }
                notes.Add(new Note() { Content = generic.note, IndentLevel = generic.indent_level });
            }

            var patchUrl = $"https://www.dota2.com/patches/{patchData.PatchNumber}";
            var description = "";
            foreach (var note in notes)
            {
                var tab = "• ";
                if (note.IndentLevel > 1)
                {
                    tab = String.Concat(Enumerable.Repeat(Emotes.Spacer.ToString(), note.IndentLevel)) + "◦ ";
                }
                description += tab + note.Content + "\n";
            }
            description.ReplaceLocalFormatting();
            var generalPatchEmbed = new Data.Models.Embeds.Embed()
            {
                Title = $"Patch {patchData.PatchNumber} - General changes",
                Description = description,
                Url = patchUrl,
                ColorRaw = Color.DarkRed,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(patchData.PatchTimestamp),
                ThumbnailUrl = $"https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/footer_logo.png",
                Footer = new() { Text = "Patch " + patchData.PatchNumber },
            };
            generalPatchEmbed.Fields = new List<Field>() { new() {
                    Name = "Full patch notes",
                    Value = $"[Click here for full patch notes]({patchUrl})" +
                    $"\nAdditionally, use the command `/patch <hero|item> <name>` to view the most recent changes for a specifc hero or item "
                }};
            return new()
            {
                Id = MakeId(patchData.PatchTimestamp.ToString(), "0", "0"), //Temp custom id
                Embed = generalPatchEmbed,
                PatchNumber = patchData.PatchNumber,
            };
        }

        public static IEnumerable<Data.Models.Embeds.HeroPatchNote> GetHeroPatchNotes(Patch patchData, RawPatchNote patchNoteData, IEnumerable<Hero> heroInfoList, IEnumerable<Ability> abilityData)
        {
            ArgumentNullException.ThrowIfNull(heroInfoList);

            var heroPatchNotesList = new List<Data.Models.Embeds.HeroPatchNote>();
            foreach (var hero in patchNoteData.heroes ?? new List<RawPatchNote.Heroes>())
            {
                var heroInfo = heroInfoList.Where(x => x.Id == hero.hero_id).First();

                var heroNotes = new List<Note>();
                var abilityList = new List<AbilityNote>();
                var talents = new List<Note>();

                foreach (var heroNote in hero.hero_notes ?? new List<RawPatchNote.Hero_notes>())
                {
                    if (heroNote.note == "<br>") continue;
                    var talentString = "Talent: ";
                    if (heroNote.note.StartsWith(talentString))
                    {
                        talents.Add(new Note() { Content = heroNote.note.Substring(talentString.Length), IndentLevel = heroNote.indent_level });
                    }
                    else
                    {
                        heroNotes.Add(new Note() { Content = heroNote.note, IndentLevel = heroNote.indent_level });
                    }
                }

                foreach (var ability in hero.abilities ?? new List<RawPatchNote.Abilities>())
                {
                    var abilityNotes = new List<Note>();
                    foreach (var abilityNote in ability.ability_notes ?? new List<RawPatchNote.Ability_notes>())
                    {
                        if (abilityNote.note == "<br>") continue;
                        abilityNotes.Add(new Note() { Content = abilityNote.note, IndentLevel = abilityNote.indent_level });
                    }
                    abilityList.Add(new AbilityNote() { AbilityId = ability.ability_id, Notes = abilityNotes });
                }

                foreach (var talent in hero.talent_notes ?? new List<RawPatchNote.Talent_notes>())
                {
                    if (talent.note == "<br>") continue;
                    talents.Add(new Note() { Content = talent.note, IndentLevel = talent.indent_level });
                }

                var fields = new List<Field>();
                if (heroNotes.Count > 0)
                {
                    var value = "";
                    foreach (var note in heroNotes)
                    {
                        var tab = "• ";
                        if (note.IndentLevel > 1)
                        {
                            tab = String.Concat(Enumerable.Repeat(Emotes.Spacer.ToString(), note.IndentLevel)) + "◦ ";
                        }
                        value += tab + note.Content + "\n";
                    }
                    value.ReplaceLocalFormatting();
                    fields.Add(new() { Name = "General:", Value = value });
                }
                foreach (var abilityNote in abilityList)
                {
                    var abilityInfo = abilityData.Where(x => x.Id == abilityNote.AbilityId).First();
                    var value = "";
                    foreach (var note in abilityNote.Notes)
                    {
                        var tab = "• ";
                        if (note.IndentLevel > 1)
                        {
                            tab = String.Concat(Enumerable.Repeat(Emotes.Spacer.ToString(), note.IndentLevel)) + "◦ ";
                        }
                        value += tab + note.Content + "\n";
                    }
                    value.ReplaceLocalFormatting();
                    fields.Add(new() { Name = $"{abilityInfo.LocalName}:", Value = value });
                }
                if (talents.Count > 0)
                {
                    var talentValue = "";
                    foreach (var note in talents)
                    {
                        var tab = "• ";
                        if (note.IndentLevel > 1)
                        {
                            tab = String.Concat(Enumerable.Repeat(Emotes.Spacer.ToString(), note.IndentLevel)) + "◦ ";
                        }
                        talentValue += tab + note.Content + "\n";
                    }
                    talentValue.ReplaceLocalFormatting();
                    fields.Add(new() { Name = "Talents:", Value = talentValue });
                }

                var patchUrl = $"https://www.dota2.com/patches/{patchData.PatchNumber}";
                var heroPatchNoteEmbed = new Data.Models.Embeds.Embed()
                {
                    Title = $"{heroInfo.LocalName} - changes {patchData.PatchNumber}",
                    Url = patchUrl,
                    ColorRaw = Color.DarkOrange,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(patchData.PatchTimestamp),
                    ThumbnailUrl = $"https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/heroes/{heroInfo.InternalName.Substring(14)}.png",
                    Footer = new() { Text = "Patch " + patchData.PatchNumber },
                    Fields = fields,
                };
                heroPatchNotesList.Add(new()
                {
                    Id = MakeId(patchData.PatchTimestamp.ToString(), "2", heroInfo.Id.ToString()), //Temp custom id
                    EntityId = heroInfo.Id,
                    LocalName = heroInfo.LocalName,
                    InternalName = heroInfo.InternalName,
                    Embed = heroPatchNoteEmbed,
                    PatchNumber = patchData.PatchNumber,
                });
            }
            return heroPatchNotesList;
        }

        public static IEnumerable<Data.Models.Embeds.ItemPatchNote> GetItemPatchNotes(Patch patchData, RawPatchNote patchNoteData, IEnumerable<Item> itemInfoList)
        {
            ArgumentNullException.ThrowIfNull(itemInfoList);

            var itemPatchNotesList = new List<Data.Models.Embeds.ItemPatchNote>();
            var items = new List<RawPatchNote.Items>();
            if (patchNoteData.items != null)
            {
                items.AddRange(patchNoteData.items);
            }
            if (patchNoteData.neutral_items != null)
            {
                items.AddRange(patchNoteData.neutral_items);
            }

            var increment = 1;
            foreach (var item in items)
            {
                var abilityList = new List<Note>();
                var itemInfo = itemInfoList.Where(x => x.Id == item.ability_id).First();

                foreach (var abilityNote in item.ability_notes ?? new List<RawPatchNote.Ability_notes>())
                {
                    if (abilityNote.note == "<br>") continue;
                    abilityList.Add(new Note() { Content = abilityNote.note, IndentLevel = abilityNote.indent_level });
                }

                var patchUrl = $"https://www.dota2.com/patches/{patchData.PatchNumber}";
                var description = "";
                foreach (var note in abilityList)
                {
                    var tab = "• ";
                    if (note.IndentLevel > 1)
                    {
                        tab = String.Concat(Enumerable.Repeat(Emotes.Spacer.ToString(), note.IndentLevel)) + "◦ ";
                    }
                    description += tab + note.Content + "\n";
                }
                description.ReplaceLocalFormatting();

                var existingNoteIndex = itemPatchNotesList.FindIndex(x => x.EntityId == item.ability_id);
                if (existingNoteIndex != -1){
                    itemPatchNotesList[existingNoteIndex].Embed.Description += description;
                    continue;
                }

                var itemPatchNoteEmbed = new Data.Models.Embeds.Embed()
                {
                    Title = $"{itemInfo.LocalName} - changes {patchData.PatchNumber}",
                    Description = description,
                    Url = patchUrl,
                    ColorRaw = Color.DarkBlue,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(patchData.PatchTimestamp),
                    ThumbnailUrl = $"https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/items/{itemInfo.InternalName.Substring(5)}.png",
                    Footer = new() { Text = "Patch " + patchData.PatchNumber}
                };
                itemPatchNotesList.Add(new()
                {
                    Id = MakeId(patchData.PatchTimestamp.ToString(), "2", itemInfo.Id.ToString()), //Temp custom id
                    EntityId = itemInfo.Id,
                    LocalName = itemInfo.LocalName,
                    InternalName = itemInfo.InternalName,
                    Embed = itemPatchNoteEmbed,
                    PatchNumber = patchData.PatchNumber,
                });
            }
            return itemPatchNotesList;
        }

        private static ulong MakeId(string timestamp, string type, string id)
            => Convert.ToUInt64(String.Format("{0}{1}{2}{3}", timestamp, type, id.PadLeft(5, '0'), "0000"));

    }
}
