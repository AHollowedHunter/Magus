using Magus.Data.Models.Dota;
using System.Text.RegularExpressions;

namespace Magus.DataBuilder
{
    internal static class Utilities
    {
        public static BaseSpell FormatSpell(this BaseSpell spell)
        {
            foreach (var value in spell.SpecialValues)
            {
                var formattedValue = "";
                if (value.ValuesInt != null && value.ValuesInt.Length != 0)
                {
                    foreach (var item in value.ValuesInt)
                    {
                        formattedValue += item.ToString();
                        if (value.IsPercentage) formattedValue += "%";
                        formattedValue += "  ";
                    }
                }
                else
                {
                    foreach (var item in value.ValuesFloat)
                    {
                        formattedValue += item.ToString();
                        if (value.IsPercentage) formattedValue += "%";
                        formattedValue += "  ";
                    }
                }
                formattedValue = formattedValue.Trim().Replace("  ", " / ");
                formattedValue = Discord.Format.Bold(formattedValue);

                spell.LocalDesc = spell.LocalDesc.Replace($"%{value.Name.ToLower()}%", formattedValue);
                spell.LocalShard = spell.LocalShard.Replace($"%{value.Name.ToLower()}%", formattedValue);
                spell.LocalScepter = spell.LocalScepter.Replace($"%{value.Name.ToLower()}%", formattedValue);

                for (var i = 0; i < spell.LocalNotes.Length; i++)
                {
                    spell.LocalNotes[i] = spell.LocalNotes[i].Replace($"%{value.Name.ToLower()}%", formattedValue);
                }

                // Some talents are missing the special values...
                spell.LocalName = spell.LocalName.Replace($"{{s:{value.Name.ToLower()}}}", formattedValue);
            }
            //  Replace percentage signs. Dirty code
            spell.LocalDesc = ReplaceLocalFormatting(spell.LocalDesc);
            spell.LocalShard = ReplaceLocalFormatting(spell.LocalShard);
            spell.LocalScepter = ReplaceLocalFormatting(spell.LocalScepter);
            for (var i = 0; i < spell.LocalNotes.Length; i++)
            {
                spell.LocalNotes[i] = ReplaceLocalFormatting(spell.LocalNotes[i]);
            }

            spell.SpecialValues = FormatSpecialValues(spell.SpecialValues);

            return spell;
        }

        public static string ReplaceLocalFormatting(this string local)
        {
            //  Replace percentage signs. Dirty code
            local = local.Replace("**%%", "****%**");
            local = local.Replace("%%", "%");
            local = local.Replace("%****%**", "%**");
            local = local.Replace("****%**", "%**");
            // Replace certain HTML elements
            var bTag = new Regex(@"(?i)<[/]?\s*b\s*/?>");
            local = bTag.Replace(local, "**");
            var brTag = new Regex(@"(?i)<\s*br\s*/?>");
            local = brTag.Replace(local, "\n");
            var miscTag = new Regex(@"(?i)<[/]?\s*(font|span)[^>]*>");
            local = miscTag.Replace(local, "**");
            var h1StartTag = new Regex(@"(?i)<\s*h1[^>]*>");
            local = h1StartTag.Replace(local, "**__");
            var h1EndTag = new Regex(@"(?i)</\s*h1[^>]*>");
            local = h1EndTag.Replace(local, "__**\n");
            return local;
        }

        private static SpecialValues[] FormatSpecialValues(SpecialValues[] specialValues)
        {
            Dictionary<string, string> replacements = new Dictionary<string, string>();

            replacements.Add("$agi", "Agility");
            replacements.Add("$str", "Strength");
            replacements.Add("$int", "Intelligence");
            replacements.Add("$all", "All Attributes");
            replacements.Add("$mana_regen", "Mana Regeneration");
            replacements.Add("$damage", "Damage");
            replacements.Add("$attack", "Attack Speed");
            replacements.Add("$spell_resist", "Magic Resistance");
            replacements.Add("$move_speed", "Movement Speed");
            replacements.Add("$armor", "Armor");
            replacements.Add("$hp_regen", "HP Regeneration");
            replacements.Add("$health", "Health");
            replacements.Add("$mana", "Mana");
            replacements.Add("$attack_range", "Attack Range");
            replacements.Add("$cast_range", "Cast Range");
            replacements.Add("$spell_amp", "Spell Damage");
            replacements.Add("$manacost_reduction", "Manacost Reduction");
            replacements.Add("$evasion", "Evasion");
            replacements.Add("$debuff_amp", "Debuff Duration");
            replacements.Add("$attack_range_melee", "Attack Range (Melee Only)");
            replacements.Add("$primary_attribute", "Primary Attribute");
            replacements.Add("$projectile_speed", "Projectile Speed");
            replacements.Add("$", "Agility");

            foreach (var value in specialValues)
            {
                if (!string.IsNullOrEmpty(value.LocalHeading))
                {
                    if (value.LocalHeading.StartsWith('+') || value.LocalHeading.StartsWith('-'))
                    {
                        string values = $"{(value.ValuesFloat != null ? string.Join(" / ", value.ValuesFloat) : string.Join(" / ", value.ValuesInt))}";
                        string text = value.LocalHeading.Substring(1);
                        if (text.StartsWith("$"))
                            foreach (var placeholder in replacements)
                                text = text.Replace(placeholder.Key, placeholder.Value);

                        value.LocalHeading = $"{value.LocalHeading[0]} **{values}** {text}";
                    }
                    else
                    {
                        string values = $"{(value.ValuesFloat != null ? string.Join(" / ", value.ValuesFloat) : string.Join(" / ", value.ValuesInt))}";
                        value.LocalHeading = $"{value.LocalHeading} **{values}**";
                    }
                }
                value.LocalHeading = ReplaceLocalFormatting(value.LocalHeading);
            }
            return specialValues;
        }
    }
}
