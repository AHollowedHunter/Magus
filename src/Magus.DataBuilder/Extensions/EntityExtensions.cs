using ACave.Extensions.Common;
using Magus.Common.Discord;
using Magus.Common.Dota;
using Magus.Common.Dota.Enums;
using Magus.Common.Dota.Models;
using Magus.Common.Emotes;
using Magus.Data.Models.V2;

namespace Magus.DataBuilder.Extensions;

public static class EntityExtensions
{
    public static EntityInfo GetHeroInfoEmbed(this Hero hero, string locale, Patch latestPatch)
    {
        var heroInfoEmbed = new SerializableEmbed()
        {
            Title        = hero.Name,
            Description  = $"> {hero.NpeDesc}",
            Url          = URLs.GetHeroUrl(hero.Name),
            ColorRaw     = 0X00A84300,
            Timestamp    = DateTimeOffset.FromUnixTimeSeconds((long)latestPatch.Timestamp),
            ThumbnailUrl = $"{URLs.Hero}{hero.InternalName.Substring(14)}.png",
            Footer       = new($"Patch {latestPatch.PatchNumber}", hero.AttributePrimary.GetAttributeIcon() ),
        };
        var heroInfoFields = new List<SerializableField>();

        // Attributes
        heroInfoFields.Add(new($"{MagusEmotes.StrengthIcon} Strength {(hero.AttributePrimary == AttributePrimary.DOTA_ATTRIBUTE_STRENGTH ? "⭐" : "")}",
            $"{hero.AttributeBaseStrength} +{hero.AttributeStrengthGain}",
            true
        ));

        heroInfoFields.Add(new($"{MagusEmotes.AgilityIcon} Agility {(hero.AttributePrimary == AttributePrimary.DOTA_ATTRIBUTE_AGILITY ? "⭐" : "")}",
            $" {hero.AttributeBaseAgility} +{hero.AttributeAgilityGain}",
            true
        ));
        heroInfoFields.Add(new($"{MagusEmotes.IntelligenceIcon} Intelligence {(hero.AttributePrimary == AttributePrimary.DOTA_ATTRIBUTE_INTELLECT ? "⭐" : "")}",
            $"{hero.AttributeBaseIntelligence} +{hero.AttributeIntelligenceGain}",
            true
        ));

        //Stats
        heroInfoFields.Add(new("Attack",
            $"{MagusEmotes.DamageIcon} {hero.GetAttackDamageMin():n0} - {hero.GetAttackDamageMax():n0}\n{MagusEmotes.AttackTimeIcon} {hero.GetAttackTime():n2}{MagusEmotes.Spacer}({hero.AttackRate:n1} Base)\n{MagusEmotes.AttackRangeIcon} {hero.AttackRange}\n{MagusEmotes.ProjectileSpeedIcon} {hero.ProjectileSpeed}",
            true
        ));
        heroInfoFields.Add(new("Defence",
            $"{MagusEmotes.ArmourIcon} {hero.GetArmor():n1}\n{MagusEmotes.MagicResistIcon} {hero.MagicalResistance}",
            true
        ));
        heroInfoFields.Add(new("Mobility",
            $"{MagusEmotes.MoveSpeedIcon} {hero.MovementSpeed}\n{MagusEmotes.TurnRateIcon} {hero.MovementTurnRate}\n{MagusEmotes.VisionIcon} {hero.VisionDaytimeRange} / {hero.VisionNighttimeRange}",
            true
        ));

        heroInfoFields.Add(new("Attack Type",
            $"{hero.AttackCapabilities.GetAttackTypeIcon()} {hero.AttackCapabilities.GetDisplayName()}",
            true
        ));

        heroInfoFields.Add(new("Complexity",
            $"{new string('\u25c6', hero.Complexity)}{new string('\u25c7', 3 - hero.Complexity)}",
            true
        ));

        var roleValues = new List<string>();
        foreach (var role in hero.Role)
        {
            if (hero.GetHightestRoles().Contains(role))
                roleValues.Add(Discord.Format.Bold(role.ToString()));
            else
                roleValues.Add(role.ToString());
        }
        var roleValue = string.Join(" | ", roleValues);
        heroInfoFields.Add(new("Roles",
            roleValue,
             true
        ));


        var abilityValues = new List<string>();
        foreach (var ability in hero.Abilities)
        {
            var name = ability.Name;
            if (ability.AbilityType == AbilityType.DOTA_ABILITY_TYPE_ULTIMATE)
                name = Discord.Format.Bold(name);

            if (ability.AbilityIsGrantedByScepter)
                name = $"{MagusEmotes.ScepterIcon}\u00A0{name}";
            else if (ability.AbilityIsGrantedByShard)
                name = $"{MagusEmotes.ShardIcon}\u00A0{name}";

            abilityValues.Add(name);
        }

        var abilityValue = String.Join(" | ", abilityValues);
        heroInfoFields.Add(new("Abilities",
             abilityValue,
            false
        ));

        heroInfoEmbed.Fields = heroInfoFields;

        return new EntityInfo(hero.InternalName, hero.Id, Data.Enums.EntityType.Hero, locale, heroInfoEmbed);
    }

    public static EntityInfo CreateAbilityInfoEmbed(this Ability ability, string locale, Patch latestPatch, Hero hero)
    {
        var embed = new SerializableEmbed()
        {
            Title        = $"{ability.Name}",
            Description  = ability.Description,
            ColorRaw     = 0x00E67E22,
            Timestamp    = DateTimeOffset.FromUnixTimeSeconds((long)latestPatch.Timestamp),
            Footer       = $"Most recent patch: {latestPatch.PatchNumber}",
            ThumbnailUrl = URLs.GetAbilityImage(ability.InternalName),
        };
        var embedFields = new List<SerializableField>();

        if (ability.AbilityIsGrantedByScepter)
        {
            embed.Description = MagusEmotes.ScepterIcon + " SCEPTER NEW ABILITY\n" + embed.Description;
        }
        else if (ability.AbilityIsGrantedByShard)
        {
            embed.Description = MagusEmotes.ShardIcon + " SHARD NEW ABILITY\n" + embed.Description;
        }

        // Ability Properties

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
            var isInline = string.IsNullOrWhiteSpace(rightEmbedFieldValue) is false;
            embedFields.Add(new(MagusEmotes.Spacer.ToString(), leftEmbedFieldValue, isInline));
        }
        if (rightEmbedFieldValue != "")
        {
            embedFields.Add(new(MagusEmotes.Spacer.ToString(), rightEmbedFieldValue, true));
        }

        // Ability spell values
        var spellEmbedValue = string.Empty;
        foreach (var spellValue in ability.DisplayedValues)
        {
            spellEmbedValue += $"{spellValue.Value}\n";
        }
        if (spellEmbedValue != string.Empty)
            embedFields.Add(new(MagusEmotes.Spacer.ToString(), spellEmbedValue));

        var cooldowns = ability.AbilityValues.FirstOrDefault(x=>x.Name.Equals("AbilityCooldown"))?.Values ?? ability.AbilityCooldown.Distinct() ;
        if (cooldowns.Count() == 0)
            cooldowns = new List<float>() { 0F };
        var cooldownString = Discord.Format.Bold(string.Join("\u202F/\u202F", cooldowns));
        var charges = ability.AbilityValues.FirstOrDefault(x => x.Name == "AbilityCharges")?.Values ?? ability.AbilityCharges ?? Enumerable.Empty<float>();
        if (!charges.All(x => x == 0))
        {
            var chargeRestoreTimes = ability.AbilityValues.FirstOrDefault(x => x.Name == "AbilityChargeRestoreTime")?.Values ?? ability.AbilityChargeRestoreTime ?? Enumerable.Empty<float>();
            cooldownString += $"\n> Charges:\u202F{Discord.Format.Bold(string.Join("\u202F/\u202F", charges))}\n> Restore:\u202F{Discord.Format.Bold(string.Join("\u202F/\u202F", chargeRestoreTimes))}";
        }

        embedFields.Add(new($"{MagusEmotes.Spacer}", $"{MagusEmotes.CooldownIcon}\u00A0{cooldownString}", true));
        if (ability.AbilityManaCost.Any())
        {
            var manaString = Discord.Format.Bold(string.Join("\u202F/\u202F", ability.AbilityManaCost.Distinct()));
            embedFields.Add(new($"{MagusEmotes.Spacer}", $"{MagusEmotes.ManaIcon}\u00A0{manaString}", true));
        }
        if (ability.AbilityHealthCost.Any())
        {
            var hpString = Discord.Format.Bold(string.Join("\u202F/\u202F", ability.AbilityHealthCost.Distinct()));
            embedFields.Add(new($"{MagusEmotes.Spacer}", $"{MagusEmotes.HpIcon}\u00A0{hpString}", true));
        }

        // Do talents

        if (ability.AbilityHasScepter)
        {
            var value = $">>> {ability.ScepterDescription}\n";
            foreach (var spellValue in ability.ScepterValues.Where(x => !string.IsNullOrEmpty(x.Description)))
            {
                value += $"{spellValue.Description}\n";
            }
            embedFields.Add(new($"{MagusEmotes.ScepterIcon} Scepter Upgrade", value));
        }
        if (ability.AbilityHasShard)
        {
            var value = $">>> {ability.ShardDescription}\n";
            foreach (var spellValue in ability.ShardValues.Where(x => !string.IsNullOrEmpty(x.Description)))
            {
                value += $"{spellValue.Description}\n";
            }
            embedFields.Add(new($"{MagusEmotes.ShardIcon} Shard Upgrade", value));
        }

        if (ability.Notes.Count > 0)
        {
            var notes = string.Empty;
            foreach (var note in ability.Notes)
            {
                notes += $"> {note}\n";
            }
            embedFields.Add(new SerializableField("Notes", notes));
        }
        embed.Fields = embedFields;


        return new EntityInfo(ability.InternalName, ability.Id, Data.Enums.EntityType.Ability, locale, embed);

        // what about in embed?
        //        HeroId = hero.Id,
        //        Scepter = ability.AbilityHasScepter || ability.AbilityIsGrantedByScepter,
        //        Shard = ability.AbilityHasShard || ability.AbilityIsGrantedByShard
    }

    public static EntityInfo CreateItemInfoEmbed(this Item item, string locale, Patch latestPatch)
    {
        var embed = new SerializableEmbed()
        {
            Title        = $"{item.Name}",
            ColorRaw     = 0x00206694,
            Timestamp    = DateTimeOffset.FromUnixTimeSeconds((long)latestPatch.Timestamp),
            Footer       = $"Most recent patch: {latestPatch.PatchNumber}",
            ThumbnailUrl = URLs.GetItemImage(item.InternalName),
        };
        var embedFields = new List<SerializableField>();

        if (item.ItemPurchasable)
            embed.Description = $"{MagusEmotes.GoldIcon}\u00A0{Discord.Format.Bold(item.ItemCost.ToString())}\n\n";
        if (item.ItemIsNeutralDrop)
            embed.Description = $"**Tier {item.ItemNeutralTier}** Neutral Item\n\n";
        embed.Description += string.Join("\n", item.DisplayedValues.Select(x => x.Value));

        // Ability Properties
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
            var isInline = string.IsNullOrWhiteSpace(rightEmbedFieldValue) is false;
            embedFields.Add(new(MagusEmotes.Spacer.ToString(), leftEmbedFieldValue, isInline));
        }
        if (rightEmbedFieldValue != "")
        {
            embedFields.Add(new(MagusEmotes.Spacer.ToString(), rightEmbedFieldValue, true));
        }

        var cooldowns = item.AbilityValues.FirstOrDefault(x=>x.Name.Equals("AbilityCooldown"))?.Values ?? item.AbilityCooldown.Distinct() ;
        if (cooldowns.Count() == 0)
            cooldowns = new List<float>() { 0F };
        var cooldownString = Discord.Format.Bold(string.Join("\u202F/\u202F", cooldowns));
        var charges = item.AbilityValues.FirstOrDefault(x => x.Name == "AbilityCharges")?.Values ?? item.AbilityCharges ?? Enumerable.Empty<float>();
        if (!charges.All(x => x == 0))
        {
            var chargeRestoreTimes = item.AbilityValues.FirstOrDefault(x => x.Name == "AbilityChargeRestoreTime")?.Values ?? item.AbilityChargeRestoreTime ?? Enumerable.Empty<float>();
            cooldownString += $"\n> Charges:\u202F{Discord.Format.Bold(string.Join("\u202F/\u202F", charges))}\n> Restore:\u202F{Discord.Format.Bold(string.Join("\u202F/\u202F", chargeRestoreTimes))}";
        }
        cooldownString = $"{MagusEmotes.CooldownIcon}\u202F{cooldownString}";

        if (item.Spells != null && item.Spells.Count > 0)
        {
            var firstSpell = true;
            foreach (var spell in item.Spells)
            {
                var spellValue = new StringBuilder()
                    .Append(">>> ")
                    .AppendLine(spell.Description.Trim());
                if (firstSpell)
                {
                    if (item.AbilityManaCost.Any())
                    {
                        var manaString = $"{MagusEmotes.ManaIcon}\u00A0{Discord.Format.Bold(string.Join("\u202F/\u202F", item.AbilityManaCost.Count == 0 ? new List<float>(){0F} : item.AbilityManaCost.Distinct()))}";
                        spellValue.Append(manaString);
                        spellValue.Append(MagusEmotes.Spacer);
                    }
                    if (item.AbilityHealthCost.Any())
                    {

                        var hpString   = $"{MagusEmotes.HpIcon}\u00A0{Discord.Format.Bold(string.Join("\u202F/\u202F", item.AbilityHealthCost.Count == 0 ? new List<float>(){0F} : item.AbilityHealthCost.Distinct()))}";
                        spellValue.Append(hpString);
                        spellValue.Append(MagusEmotes.Spacer);
                    }
                    spellValue.Append(cooldownString);
                    firstSpell = false;
                }
                embedFields.Add(new SerializableField(spell.Name, spellValue.ToString()));
            }
        }

        if (item.Notes.Count > 0)
        {
            var notes = string.Empty;
            foreach (var note in item.Notes)
            {
                notes += $"> {note}\n";
            }
            embedFields.Add(new SerializableField("Notes", notes));
        }
        embed.Fields = embedFields;

        return new EntityInfo(item.InternalName, item.Id, Data.Enums.EntityType.Item, locale, embed);
        
    }
}
