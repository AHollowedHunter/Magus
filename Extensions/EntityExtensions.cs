using Discord;
using Magus.Common.Extensions;
using Magus.Data;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using System.Text;
using System.Text.RegularExpressions;

namespace Magus.DataBuilder.Extensions
{
    public static class EntityExtensions
    {
        private static readonly string _patchUrlBase = "https://www.dota2.com/patches/";

        public static IEnumerable<HeroInfoEmbed> GetHeroInfoEmbeds(this Hero hero, Dictionary<string, string[]> languageMap, Patch latestPatch)
        {
            var heroInfoEmbed = new Data.Models.Embeds.Embed()
            {
                Title        = hero.Name,
                Description  = hero.NpeDesc,
                Url          = DotaUrls.GetHeroUrl(hero.Name),
                ColorRaw     = 0X00A84300,
                Timestamp    = DateTimeOffset.FromUnixTimeSeconds((long)latestPatch.Timestamp),
                ThumbnailUrl = $"{DotaUrls.Hero}{hero.InternalName.Substring(14)}.png",
                Footer       = new() { Text = $"Patch {latestPatch.PatchNumber}" },
            };
            var heroInfoFields = new List<Field>();

            // Attributes
            heroInfoFields.Add(new()
            {
                Name     = $"{Emotes.StrengthIcon} Strength {(hero.AttributePrimary == AttributePrimary.DOTA_ATTRIBUTE_STRENGTH ? "⭐" : "")}",
                Value    = $"{hero.AttributeBaseStrength} +{hero.AttributeStrengthGain}",
                IsInline = true
            });

            heroInfoFields.Add(new()
            {
                Name     = $"{Emotes.AgilityIcon} Agility {(hero.AttributePrimary == AttributePrimary.DOTA_ATTRIBUTE_AGILITY ? "⭐" : "")}",
                Value    = $" {hero.AttributeBaseAgility} +{hero.AttributeAgilityGain}",
                IsInline = true
            });
            heroInfoFields.Add(new()
            {
                Name     = $"{Emotes.IntelligenceIcon} Intelligence {(hero.AttributePrimary == AttributePrimary.DOTA_ATTRIBUTE_INTELLECT ? "⭐" : "")}",
                Value    = $"{hero.AttributeBaseIntelligence} +{hero.AttributeIntelligenceGain}",
                IsInline = true
            });

            //Stats
            heroInfoFields.Add(new()
            {
                Name     = "Attack",
                Value    = $"{Emotes.DamageIcon} {hero.AttackDamageMin}-{hero.AttackDamageMax}\n{Emotes.AttackTimeIcon} {hero.AttackRate}\n{Emotes.AttackRangeIcon} {hero.AttackRange}\n{hero.AttackCapabilities.GetAttackTypeIcon()} {hero.AttackCapabilities.GetDisplayName()}",
                IsInline = true
            });
            heroInfoFields.Add(new()
            {
                Name     = "Defence",
                Value    = $"{Emotes.ArmourIcon} {hero.ArmorPhysical.ToString("n1")}\n{Emotes.MagicResistIcon} {hero.MagicalResistance}",
                IsInline = true
            });
            heroInfoFields.Add(new()
            {
                Name     = "Mobility",
                Value    = $"{Emotes.MoveSpeedIcon} {hero.MovementSpeed}\n{Emotes.TurnRateIcon} {hero.MovementTurnRate}\n{Emotes.VisionIcon} {hero.VisionDaytimeRange} / {hero.VisionNighttimeRange}",
                IsInline = true
            });

            var abilityValue = "";
            foreach (var ability in hero.Abilities)
            {
                if (ability.AbilityIsGrantedByScepter)
                {
                    abilityValue += ability.Name + Emotes.ScepterIcon + "   ";
                }
                else if (ability.AbilityIsGrantedByShard)
                {
                    abilityValue += ability.Name + Emotes.ShardIcon + "   ";
                }
                else
                {
                    abilityValue += ability.Name + "   ";
                }
            }
            // Break hero abilities across new lines, except for certain heroes
            if (hero.Name == "Invoker")
            {
                abilityValue = abilityValue.Trim().Replace("   ", " | ");
            }
            else
            {
                abilityValue = abilityValue.Trim().Replace("   ", "\n");
            }
            heroInfoFields.Add(new()
            {
                Name     = "Abilities",
                Value    = abilityValue,
                IsInline = true
            });

            heroInfoFields.Add(new()
            {
                Name     = "Complexity",
                Value    = $"{new string('\u25c6', hero.Complexity)}{new string('\u25c7', 3 - hero.Complexity)}",
                IsInline = true
            });
            var roleValue = "";
            foreach (var role in hero.GetAllRoles())
            {
                if (hero.GetHightestRoles().Contains(role))
                    roleValue += Format.Bold(role.ToString()) + " ";
                else
                    roleValue += role + " ";
            }
            roleValue = roleValue.Trim().Replace(" ", " | ");
            heroInfoFields.Add(new()
            {
                Name     = "Roles",
                Value    = roleValue,
                IsInline = true,
            });

            heroInfoFields.Add(new()
            {
                Name  = "About",
                Value = hero.Hype,
            });

            heroInfoEmbed.Fields = heroInfoFields;

            var heroInfoEmbedList = new List<HeroInfoEmbed>();
            foreach (var locale in languageMap[hero.Language])
            {
                heroInfoEmbedList.Add(new()
                {
                    Id           = GetEntityId(hero.Id, hero.InternalName, locale),
                    EntityId     = hero.Id,
                    Locale       = locale,
                    InternalName = hero.InternalName,
                    Embed        = heroInfoEmbed,
                });
            }
            return heroInfoEmbedList;
        }

        //private static string CreateFormattedDescription(BaseSpell notes, int maxLength = 4096)
        //{
        //    var description = string.Empty;
        //    string truncatedMessage = "***See website for full patchnote***";
        //    foreach (var note in notes)
        //    {
        //        var indent = notes.Any(x=> x.Indent == 0) ? note.Indent : note.Indent - 1; // Some set of notes are all indedented, so remove a level
        //        var tab = string.Empty;

        //        if (!Regex.Match(note.Value, @"^\s+$").Success)
        //        {
        //            tab = GetTab(indent);
        //        }

        //        var valueToAdd = tab + note.Value + "\n";
        //        if (!string.IsNullOrEmpty(note.Info))
        //        {
        //            valueToAdd += $"{GetTab(indent + 1)}{note.Info}\n";
        //        }

        //        if (description.Length + valueToAdd.Length + truncatedMessage.Length > maxLength)
        //        {
        //            description += truncatedMessage;
        //            break;
        //        }
        //        description += valueToAdd;
        //    }
        //    return description;
        //}

        private static string GetTab(int indent)
        {
            if (indent > 0)
            {
                return String.Concat(Enumerable.Repeat(Emotes.Spacer.ToString(), indent)) + "◦ ";
            }
            return "• ";
        }

        private static ulong GetEntityId(int entityId, string internalName, string locale)
        {
            var str = $"{entityId}_{internalName}_{locale}";
            var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            var id = BitConverter.ToUInt64(hash);
            return id;
        }
    }
}
