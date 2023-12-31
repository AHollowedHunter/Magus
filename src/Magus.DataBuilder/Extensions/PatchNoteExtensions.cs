using Discord;
using Magus.Common.Discord;
using Magus.Common.Dota;
using Magus.Common.Dota.Models;
using Magus.Common.Emotes;
using Magus.Data.Extensions;
using Magus.Data.Models.Embeds;
using Magus.Data.Models.V2;
using System.Text.RegularExpressions;

namespace Magus.DataBuilder.Extensions;

public static class PatchNoteExtensions
{
    private static readonly string _patchUrlBase = "https://www.dota2.com/patches/";

    public static PatchNote GetGeneralPatchNoteEmbeds(this PatchNotes patch, string locale)
    {
        var generalPatchEmbed = new SerializableEmbed()
        {
            Title        = $"Patch {patch.PatchName} - General changes",
            Description  = CreateFormattedDescription(patch.GenericNotes), // LIMIT IT
            Url          = _patchUrlBase + patch.PatchName,
            ColorRaw     = Color.DarkRed,
            Timestamp    = DateTimeOffset.FromUnixTimeSeconds((long)patch.Timestamp),
            ThumbnailUrl = URLs.DotaColourLogo,
            Footer       = "Patch " + patch.PatchName,
        };
        var fields = new List<SerializableField>();

        if (!string.IsNullOrEmpty(patch.Website))
        {
            fields.Add(new("Patch Website",
                $"[{patch.Website}](https://www.dota2.com/{patch.Website})"
            ));
        }
        fields.Add(new("Full patch notes",
             $"[Click here for full patch notes]({generalPatchEmbed.Url})" +
             $"\nAdditionally, use the command `/patch <hero|item> <name>` to view the most recent changes for a specifc hero or item "
        ));

        generalPatchEmbed.Fields = fields;
        return patch.CreateGeneralNote(locale, generalPatchEmbed);
    }

    public static IEnumerable<PatchNote> GetHeroPatchNoteEmbeds(this PatchNotes patch, IEnumerable<EntityInfo> heroes, Dictionary<(string Language, string Key), string> abilityValues, string locale)
    {
        var heroPatchNotesList = new List<PatchNote>();
        foreach (var hero in patch.HeroesNotes)
        {
            var heroInfo = heroes.Where(x => x.InternalName == hero.InternalName).First();

            var fields = new List<SerializableField>();

            foreach (var abilityNote in hero.AbilityNotes)
            {
                var abilityName = GetLanguageValue(abilityValues, patch.Language, abilityNote.InternalName);
                fields.Add(new($"{abilityName}:", CreateFormattedDescription(abilityNote.Notes)));
            }

            if (hero.TalentNotes.Count > 0)
            {
                fields.Add(new("Talents:", CreateFormattedDescription(hero.TalentNotes)));
            }

            var heroPatchNoteEmbed = new SerializableEmbed()
            {
                Title        = $"{heroInfo.Embed.Title} - changes {patch.PatchName}", // TODO use Entity instead?
                Description  = CreateFormattedDescription(hero.GeneralNotes),
                Url          = _patchUrlBase + patch.PatchName,
                ColorRaw     = Color.DarkOrange,
                Timestamp    = DateTimeOffset.FromUnixTimeSeconds((long)patch.Timestamp),
                ThumbnailUrl = URLs.GetHeroImage(hero.InternalName), // Store this in a hero object?
                Footer       = "Patch " + patch.PatchName,
                Fields       = fields,
            };

            heroPatchNotesList.Add(patch.CreateHeroNote(locale, heroInfo.InternalName, heroInfo.EntityId, heroPatchNoteEmbed));
        }
        return heroPatchNotesList;
    }

    public static IEnumerable<PatchNote> GetItemPatchNoteEmbeds(this PatchNotes patch, IEnumerable<EntityInfo> items, string locale)
    {
        var itemPatchNotesList = new List<PatchNote>();

        foreach (var item in patch.ItemNotes.Concat(patch.NeutralItemNotes))
        {
            var itemInfo = items.Where(x => x.InternalName == item.InternalName).FirstOrDefault();
            if (itemInfo == null) continue;

            var itemPatchNoteEmbed = new SerializableEmbed()
            {
                Title        = $"{itemInfo.Embed.Title} - changes {patch.PatchName}",
                Description  = CreateFormattedDescription(item.Notes),
                Url          = _patchUrlBase + patch.PatchName,
                ColorRaw     = Color.DarkBlue,
                Timestamp    = DateTimeOffset.FromUnixTimeSeconds((long)patch.Timestamp),
                ThumbnailUrl = URLs.GetItemImage(item.InternalName),
                Footer       = "Patch " + patch.PatchName
            };

            itemPatchNotesList.Add(patch.CreateItemNote(locale, itemInfo.InternalName, itemInfo.EntityId, itemPatchNoteEmbed));
        }
        return itemPatchNotesList;
    }

    private static string CreateFormattedDescription(IList<PatchNotes.Note> notes, int maxLength = 4096)
    {
        var description = string.Empty;
        string truncatedMessage = "***See website for full patchnote***";
        foreach (var note in notes)
        {
            var indent = notes.Any(x => x.Indent == 0) ? note.Indent : note.Indent - 1; // Some set of notes are all indedented, so remove a level
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
            return string.Concat(Enumerable.Repeat(MagusEmotes.Spacer.ToString(), indent)) + "◦ ";
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

    private static string GetLanguageValue(Dictionary<(string Language, string Key), string> values, string language, string internalName, string defaultLanguage = "english")
    {
        var key = (Language: language, Key: $"DOTA_Tooltip_ability_{internalName}".ToLower());
        if (values.TryGetValue(key, out string? value))
        {
            return value;
        }
        else
        {
            values.TryGetValue((defaultLanguage, key.Key), out value);
            return value ?? "";
        }
    }
}
