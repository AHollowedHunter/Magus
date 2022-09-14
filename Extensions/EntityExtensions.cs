using Magus.Common.Extensions;
using Magus.Data;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using System.Text;

namespace Magus.DataBuilder.Extensions
{
    public static class EntityExtensions
    {
        public static IEnumerable<HeroInfoEmbed> GetHeroInfoEmbeds(this Hero hero, Dictionary<string, string[]> languageMap, Patch latestPatch)
        {
            var heroInfoEmbed = new Embed()
            {
                Title        = hero.Name,
                Description  = $"> {hero.NpeDesc}",
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
                Value    = $"{Emotes.DamageIcon} {hero.GetAttackDamageMin()} - {hero.GetAttackDamageMax()}\n{Emotes.AttackTimeIcon} {hero.GetAttackTime().ToString("n2")}{Emotes.Spacer}({hero.AttackRate.ToString("n1")} Base)\n{Emotes.AttackRangeIcon} {hero.AttackRange}\n{Emotes.ProjectileSpeedIcon} {hero.ProjectileSpeed}",
                IsInline = true
            });
            heroInfoFields.Add(new()
            {
                Name     = "Defence",
                Value    = $"{Emotes.ArmourIcon} {hero.GetArmor().ToString("n1")}\n{Emotes.MagicResistIcon} {hero.MagicalResistance}",
                IsInline = true
            });
            heroInfoFields.Add(new()
            {
                Name     = "Mobility",
                Value    = $"{Emotes.MoveSpeedIcon} {hero.MovementSpeed}\n{Emotes.TurnRateIcon} {hero.MovementTurnRate}\n{Emotes.VisionIcon} {hero.VisionDaytimeRange} / {hero.VisionNighttimeRange}",
                IsInline = true
            });

            heroInfoFields.Add(new()
            {
                Name     = "Attack Type",
                Value    = $"{hero.AttackCapabilities.GetAttackTypeIcon()} {hero.AttackCapabilities.GetDisplayName()}",
                IsInline = true
            });

            heroInfoFields.Add(new()
            {
                Name     = "Complexity",
                Value    = $"{new string('\u25c6', hero.Complexity)}{new string('\u25c7', 3 - hero.Complexity)}",
                IsInline = true
            });

            var roleValues = new List<string>();
            foreach (var role in hero.Role)
            {
                if (hero.GetHightestRoles().Contains(role))
                    roleValues.Add(Discord.Format.Bold(role.ToString()));
                else
                    roleValues.Add(role.ToString());
            }
            var roleValue = string.Join(" | ", roleValues);
            heroInfoFields.Add(new()
            {
                Name     = "Roles",
                Value    = roleValue,
                IsInline = true,
            });


            var abilityValues = new List<string>();
            foreach (var ability in hero.Abilities)
            {
                var name = ability.Name;
                if (ability.AbilityType == AbilityType.DOTA_ABILITY_TYPE_ULTIMATE)
                    name = Discord.Format.Bold(name);

                if (ability.AbilityIsGrantedByScepter)
                    name = Emotes.ScepterIcon + name;
                else if (ability.AbilityIsGrantedByShard)
                    name = Emotes.ShardIcon + name;

                abilityValues.Add(name);
            }

            var abilityValue = String.Join(" | ", abilityValues);
            heroInfoFields.Add(new()
            {
                Name     = "Abilities",
                Value    = abilityValue,
                IsInline = false
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
                    Aliases      = hero.NameAliases,
                    RealName     = hero.RealName,
                    Name         = hero.Name,
                    Embed        = heroInfoEmbed,
                });
            }
            return heroInfoEmbedList;
        }

        public static IEnumerable<AbilityInfoEmbed> CreateAbilityInfoEmbeds(this Ability ability, Dictionary<string, string[]> languageMap, Patch latestPatch)
        {
            var embed = new Embed()
            {
                Title        = $"{ability.Name}",
                Description  = ability.Description,
                ColorRaw     = 0x00E67E22,
                Timestamp    = DateTimeOffset.FromUnixTimeSeconds((long)latestPatch.Timestamp),
                Footer       = new Footer() { Text = $"Most recent patch: {latestPatch.PatchNumber}" },
                ThumbnailUrl = DotaUrls.GetAbilityImage(ability.InternalName),
            };
            var embedFields = new List<Field>();

            if (ability.AbilityIsGrantedByScepter)
            {
                embed.Description = Emotes.ScepterIcon + " SCEPTER NEW ABILITY\n" + embed.Description;
            }
            else if (ability.AbilityIsGrantedByShard)
            {
                embed.Description = Emotes.ShardIcon + " SHARD NEW ABILITY\n" + embed.Description;
            }

            // Ability Properties

            var leftEmbedField       = new Field() { Name = Emotes.Spacer.ToString(), IsInline = true };
            var rightEmbedField      = new Field() { Name = Emotes.Spacer.ToString(), IsInline = true };
            var leftEmbedFieldValue  = "";
            var rightEmbedFieldValue = "";

            // Add Logic for targetType
            leftEmbedFieldValue += $"Ability: **{(ability.HasBehaviour(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_AOE) ? "AoE " : string.Empty)}{string.Join(" | ", ability.GetTargetTypeNames())}**\n";
            if (ability.AbilityUnitTargetTeam.GetDisplayName() != null)
            {
                leftEmbedFieldValue += $"Affects: **{ability.AbilityUnitTargetTeam.GetDisplayName()}**\n";
            }
            if (ability.AbilityUnitDamageType != AbilityUnitDamageType.DAMAGE_TYPE_NONE)
            {
                leftEmbedFieldValue += $"Damage type: **{ability.AbilityUnitDamageType.GetDisplayName()}**\n";
            }
            if (ability.SpellImmunityType != SpellImmunityType.SPELL_IMMUNITY_NONE)
            {
                rightEmbedFieldValue += $"Pierces Spell Immunity: **{ability.SpellImmunityType.GetDisplayName()}**\n";
            }
            if (ability.SpellDispellableType != SpellDispellableType.SPELL_DISPELLABLE_NONE)
            {
                rightEmbedFieldValue += $"Dispellable: **{ability.SpellDispellableType.GetDisplayName()}**\n";
            }

            if (leftEmbedFieldValue != "")
            {
                if (string.IsNullOrWhiteSpace(rightEmbedFieldValue))
                    leftEmbedField.IsInline = false; //Do this to stop horrible inline IF ability values are 0
                leftEmbedField.Value = leftEmbedFieldValue;
                embedFields.Add(leftEmbedField);
            }
            if (rightEmbedFieldValue != "")
            {
                rightEmbedField.Value = rightEmbedFieldValue;
                embedFields.Add(rightEmbedField);
            }

            // Ability spell values
            var spellEmbed = new Field() { Name = Emotes.Spacer.ToString(), Value = string.Empty };
            foreach (var spellValue in ability.DisplayedValues)
            {
                spellEmbed.Value += $"{spellValue.Value}\n";
            }
            if (spellEmbed.Value != string.Empty)
                embedFields.Add(spellEmbed);

            var cooldowns = ability.AbilityValues.FirstOrDefault(x=>x.Name.Equals("AbilityCooldown"))?.Values ?? ability.AbilityCooldown.Distinct() ;
            if (cooldowns.Count() == 0)
                cooldowns = new List<float>() { 0F };
            var cooldownString = Discord.Format.Bold(string.Join("\u00A0/\u00A0", cooldowns));
            var charges = ability.AbilityValues.FirstOrDefault(x => x.Name == "AbilityCharges")?.Values ?? ability.AbilityCharges ?? Enumerable.Empty<float>();
            if (!charges.All(x => x == 0))
            {
                var chargeRestoreTimes = ability.AbilityValues.FirstOrDefault(x => x.Name == "AbilityChargeRestoreTime")?.Values ?? ability.AbilityChargeRestoreTime ?? Enumerable.Empty<float>();
                cooldownString += $"\n> Charges:\u00A0{Discord.Format.Bold(string.Join("\u00A0/\u00A0", charges))}\n> Restore:\u00A0{Discord.Format.Bold(string.Join("\u00A0/\u00A0", chargeRestoreTimes))}";
            }
            var manaString     = Discord.Format.Bold(string.Join("\u00A0/\u00A0", ability.AbilityManaCost.Count == 0 ? new List<float>(){0F} : ability.AbilityManaCost.Distinct()));
            embedFields.Add(new Field() { Name = $"{Emotes.Spacer}", IsInline = true, Value = $"{Emotes.CooldownIcon}\u00A0{cooldownString}" });
            embedFields.Add(new Field() { Name = $"{Emotes.Spacer}", IsInline = true, Value = $"{Emotes.ManaIcon}\u00A0{manaString}" });

            // Do talents

            if (ability.AbilityHasScepter)
            {
                var value = $">>> {ability.ScepterDescription}\n";
                foreach (var spellValue in ability.ScepterValues.Where(x => !string.IsNullOrEmpty(x.Description)))
                {
                    value += $"{spellValue.Description}\n";
                }
                embedFields.Add(new Field() { Name = $"{Emotes.ScepterIcon} Scepter Upgrade", Value = value });
            }
            if (ability.AbilityHasShard)
            {
                var value = $">>> {ability.ShardDescription}\n";
                foreach (var spellValue in ability.ShardValues.Where(x => !string.IsNullOrEmpty(x.Description)))
                {
                    value += $"{spellValue.Description}\n";
                }
                embedFields.Add(new Field() { Name = $"{Emotes.ShardIcon} Shard Upgrade", Value = value });
            }

            if (ability.Notes.Count > 0)
            {
                var notesField = new Field() {Name = "Notes", Value = ""};
                foreach (var note in ability.Notes)
                {
                    notesField.Value += $"> {note}\n";
                }
                embedFields.Add(notesField);
            }
            embed.Fields = embedFields;

            var abilityInfoEmbedList = new List<AbilityInfoEmbed>();
            foreach (var locale in languageMap[ability.Language])
            {
                abilityInfoEmbedList.Add(new()
                {
                    Id           = GetEntityId(ability.Id, ability.InternalName, locale),
                    EntityId     = ability.Id,
                    Locale       = locale,
                    InternalName = ability.InternalName,
                    Name         = ability.Name,
                    Embed        = embed,
                });
            }
            return abilityInfoEmbedList;
        }

        public static IEnumerable<ItemInfoEmbed> CreateItemInfoEmbeds(this Item item, Dictionary<string, string[]> languageMap, Patch latestPatch)
        {
            var embed = new Embed()
            {
                Title        = $"{item.Name}",
                ColorRaw     = 0x00206694,
                Timestamp    = DateTimeOffset.FromUnixTimeSeconds((long)latestPatch.Timestamp),
                Footer       = new Footer() { Text = $"Most recent patch: {latestPatch.PatchNumber}" },
                ThumbnailUrl = DotaUrls.GetItemImage(item.InternalName),
            };
            var embedFields = new List<Field>();

            if (item.ItemPurchasable)
                embed.Description = $"{Emotes.GoldIcon}\u00A0{Discord.Format.Bold(item.ItemCost.ToString())}\n\n";
            if (item.ItemIsNeutralDrop)
                embed.Description = $"**Tier {item.ItemNeutralTier}** Neutral Item\n\n";
            embed.Description += String.Join("\n", item.DisplayedValues.Select(x => x.Value));

            // Ability Properties
            var leftEmbedField       = new Field() { Name = Emotes.Spacer.ToString(), IsInline = true };
            var rightEmbedField      = new Field() { Name = Emotes.Spacer.ToString(), IsInline = true };
            var leftEmbedFieldValue  = "";
            var rightEmbedFieldValue = "";

            // Add Logic for targetType
            leftEmbedFieldValue += $"Ability: **{(item.HasBehaviour(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_AOE) ? "AoE " : string.Empty)}{string.Join(" | ", item.GetTargetTypeNames())}**\n";
            if (item.AbilityUnitTargetTeam.GetDisplayName() != null)
            {
                leftEmbedFieldValue += $"Affects: **{item.AbilityUnitTargetTeam.GetDisplayName()}**\n";
            }
            if (item.AbilityUnitDamageType != AbilityUnitDamageType.DAMAGE_TYPE_NONE)
            {
                leftEmbedFieldValue += $"Damage type: **{item.AbilityUnitDamageType.GetDisplayName()}**\n";
            }
            if (item.SpellImmunityType != SpellImmunityType.SPELL_IMMUNITY_NONE)
            {
                rightEmbedFieldValue += $"Pierces Spell Immunity: **{item.SpellImmunityType.GetDisplayName()}**\n";
            }
            if (item.SpellDispellableType != SpellDispellableType.SPELL_DISPELLABLE_NONE)
            {
                rightEmbedFieldValue += $"Dispellable: **{item.SpellDispellableType.GetDisplayName()}**\n";
            }

            if (leftEmbedFieldValue != "")
            {
                if (string.IsNullOrWhiteSpace(rightEmbedFieldValue))
                    leftEmbedField.IsInline = false; //Do this to stop horrible inline IF ability values are 0
                leftEmbedField.Value = leftEmbedFieldValue;
                embedFields.Add(leftEmbedField);
            }
            if (rightEmbedFieldValue != "")
            {
                rightEmbedField.Value = rightEmbedFieldValue;
                embedFields.Add(rightEmbedField);
            }

            var cooldowns = item.AbilityValues.FirstOrDefault(x=>x.Name.Equals("AbilityCooldown"))?.Values ?? item.AbilityCooldown.Distinct() ;
            if (cooldowns.Count() == 0)
                cooldowns = new List<float>() { 0F };
            var cooldownString = Discord.Format.Bold(string.Join("\u00A0/\u00A0", cooldowns));
            var charges = item.AbilityValues.FirstOrDefault(x => x.Name == "AbilityCharges")?.Values ?? item.AbilityCharges ?? Enumerable.Empty<float>();
            if (!charges.All(x => x == 0))
            {
                var chargeRestoreTimes = item.AbilityValues.FirstOrDefault(x => x.Name == "AbilityChargeRestoreTime")?.Values ?? item.AbilityChargeRestoreTime ?? Enumerable.Empty<float>();
                cooldownString += $"\n> Charges:\u00A0{Discord.Format.Bold(string.Join("\u00A0/\u00A0", charges))}\n> Restore:\u00A0{Discord.Format.Bold(string.Join("\u00A0/\u00A0", chargeRestoreTimes))}";
            }
            cooldownString = $"{Emotes.CooldownIcon}\u00A0{cooldownString}";
            var manaString = $"{Emotes.ManaIcon}\u00A0{Discord.Format.Bold(string.Join("\u00A0/\u00A0", item.AbilityManaCost.Count == 0 ? new List<float>(){0F} : item.AbilityManaCost.Distinct()))}";

            var cooldownAndManaFields = new List<Field>() {
                new Field() { Name = cooldownString, IsInline = true, Value = $"{Emotes.Spacer}" },
                new Field() { Name = manaString, IsInline = true, Value = $"{Emotes.Spacer}" }
            };

            if (item.Spells != null && item.Spells.Count > 0)
            {
                var firstSpell = true;
                foreach (var spell in item.Spells)
                {
                    var spellField = new Field() { Name = spell.Name, Value = $">>> {spell.Description.Trim()}" };
                    if (firstSpell)
                    {
                        spellField.Value += $"\n{manaString}{Emotes.Spacer}{cooldownString}";
                        firstSpell = false;
                    }
                    embedFields.Add(spellField);
                }
            }

            if (item.Notes.Count > 0)
            {
                var notesField = new Field() {Name = "Notes", Value = ""};
                foreach (var note in item.Notes)
                {
                    notesField.Value += $"> {note}\n";
                }
                embedFields.Add(notesField);
            }
            embed.Fields = embedFields;

            var itemInfoEmbedList = new List<ItemInfoEmbed>();
            foreach (var locale in languageMap[item.Language])
            {
                itemInfoEmbedList.Add(new()
                {
                    Id           = GetEntityId(item.Id, item.InternalName, locale),
                    EntityId     = item.Id,
                    Locale       = locale,
                    InternalName = item.InternalName,
                    Name         = item.Name,
                    Embed        = embed,
                });
            }
            return itemInfoEmbedList;
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
