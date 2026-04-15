using AngleSharp.Text;
using Magus.Common.Dota.Enums;
using Magus.Common.Dota.ModelsV2;
using Magus.Common.Dota.ModelsV2.AbilityValue;
using UltimyrArchives.Updater.Extensions;
using ValveResourceFormat.Serialization.KeyValues;

namespace UltimyrArchives.Updater.Converters;

public sealed class UnitAbilityConverter(KVObject baseAbility, Dictionary<string, int> unitAbilityIds) : AbilityConverter<UnitAbility>(baseAbility)
{
    public override UnitAbility Convert(string name, KVObject ability)
    {
        var abilityType = ability.GetEnumOrDefault("AbilityType", BaseAbility.AbilityType);

        byte maxLevel = 4;
        if (ability.TryGetValue("MaxLevel", out var kvLevel))
            maxLevel = kvLevel.ToByte(CultureInfo.InvariantCulture);
        else if (name == "meepo_divided_we_stand")
            maxLevel = 4;
        else if (abilityType is AbilityType.ABILITY_TYPE_ULTIMATE)
            maxLevel = 3;

        return new UnitAbility
        {
            InternalName          = name,
            Id                    = unitAbilityIds[name],
            AbilityValues         = ability.GetValueOrDefault("AbilityValues", []).Select(UnitAbilityValueConverter).ToArray(),
            AbilitySharedCooldown = ability.GetStringOrDefault("AbilitySharedCooldown", formatProvider: CultureInfo.InvariantCulture),
            MaxLevel              = maxLevel,

            // Enums
            AbilityType            = abilityType,
            AbilityBehavior        = ability.GetEnumOrDefault("AbilityBehavior", BaseAbility.AbilityBehavior),
            AbilityUnitDamageType  = ability.GetEnumOrDefault<AbilityUnitDamageType>("AbilityUnitDamageType"),
            AbilityUnitTargetTeam  = ability.GetEnumOrDefault<AbilityUnitTargetTeam>("AbilityUnitTargetTeam"),
            AbilityUnitTargetType  = ability.GetEnumOrDefault<AbilityUnitTargetType>("AbilityUnitTargetType"),
            AbilityUnitTargetFlags = ability.GetEnumOrDefault<AbilityUnitTargetFlags>("AbilityUnitTargetFlags"),
            SpellImmunityType      = ability.GetEnumOrDefault<SpellImmunityType>("SpellImmunityType"),
            SpellDispellableType   = ability.GetEnumOrDefault<SpellDispellableType>("SpellDispellableType"),

            // Stats
            AbilityCastRange          = ability.GetValueOrDefault("AbilityCastRange")?.ParseArray<float>() ?? BaseAbility.AbilityCastRange,
            AbilityOvershootCastRange = ability.GetValueOrDefault("AbilityOvershootCastRange")?.ParseArray<float>() ?? BaseAbility.AbilityOvershootCastRange,
            AbilityCastRangeBuffer    = ability.GetValueOrDefault("AbilityCastRangeBuffer")?.ParseArray<float>() ?? BaseAbility.AbilityCastRangeBuffer,
            AbilityCastPoint          = ability.GetValueOrDefault("AbilityCastPoint")?.ParseArray<float>() ?? BaseAbility.AbilityCastPoint,
            AbilityChannelTime        = ability.GetValueOrDefault("AbilityChannelTime")?.ParseArray<float>() ?? BaseAbility.AbilityChannelTime,
            AbilityCooldown           = ability.GetValueOrDefault("AbilityCooldown")?.ParseArray<float>() ?? BaseAbility.AbilityCooldown,
            AbilityDuration           = ability.GetValueOrDefault("AbilityDuration")?.ParseArray<float>() ?? BaseAbility.AbilityDuration,
            AbilityCharges            = ability.GetValueOrDefault("AbilityCharges")?.ParseArray<float>() ?? BaseAbility.AbilityCharges,
            AbilityChargeRestoreTime  = ability.GetValueOrDefault("AbilityChargeRestoreTime")?.ParseArray<float>() ?? BaseAbility.AbilityChargeRestoreTime,
            AbilityDamage             = ability.GetValueOrDefault("AbilityDamage")?.ParseArray<float>() ?? BaseAbility.AbilityDamage,
            AbilityManaCost           = ability.GetValueOrDefault("AbilityManaCost")?.ParseArray<float>() ?? BaseAbility.AbilityManaCost,
            AbilityHealthCost         = ability.GetValueOrDefault("AbilityHealthCost")?.ParseArray<float>() ?? BaseAbility.AbilityHealthCost,

            // Unit Ability
            IsBreakable        = ability.GetBooleanOrDefault("IsBreakable", formatProvider: CultureInfo.InvariantCulture),
            IsGrantedByScepter = ability.GetBooleanOrDefault("IsGrantedByScepter", formatProvider: CultureInfo.InvariantCulture),
            HasScepterUpgrade  = ability.GetBooleanOrDefault("HasScepterUpgrade", formatProvider: CultureInfo.InvariantCulture),
            IsGrantedByShard   = ability.GetBooleanOrDefault("IsGrantedByShard", formatProvider: CultureInfo.InvariantCulture),
            HasShardUpgrade    = ability.GetBooleanOrDefault("HasShardUpgrade", formatProvider: CultureInfo.InvariantCulture),
        };
    }

    private static IAbilityValue UnitAbilityValueConverter(KVOPair kvoPair)
    {
        (string name, KVObject kvObject) = kvoPair;
        switch (kvObject.Count)
        {
            case 0:
                return new BasicValue(name, kvObject.ParseArray<float>());
            case <= 2 when kvObject.All(x => BasicValue.Keys.Contains(x.Key)):
                return new BasicValue(
                    name,
                    kvObject["value"].ParseArray<float>(ignoreNonNumericChars: true),
                    kvObject.GetBooleanOrDefault("affected_by_aoe_increase"));
        }

        SpecialBonus[]? specialBonus = null;
        if (kvObject.Where(x => Rx.SpecialBonus.IsMatch(x.Key)) is { } specialBonuses)
            specialBonus =
            [
                ..specialBonuses.Select(x => new SpecialBonus(x.Key, SpecialBonusValue.Parse(x.Value.ToString(CultureInfo.InvariantCulture).Split())))
            ];
        return new HeroesAbilityValue
        {
            Name                        = name,
            Value                       = kvObject.GetValueOrDefault("value")?.ParseArray<float>() ?? [],
            AffectedByAOEIncrease       = kvObject.GetBooleanOrDefault("affected_by_aoe_increase"),
            SpecialBonuses              = specialBonus,
            Innate                      = kvObject.GetBooleanProperty("Innate"),
            RequiresScepter             = kvObject.GetBooleanProperty("RequiresScepter"),
            RequiresShard               = kvObject.GetBooleanProperty("RequiresShard"),
            LinkedSpecialBonus          = kvObject.GetStringOrDefault("LinkedSpecialBonus", formatProvider: CultureInfo.InvariantCulture),
            LinkedSpecialBonusField     = kvObject.GetStringOrDefault("LinkedSpecialBonusField", formatProvider: CultureInfo.InvariantCulture),
            LinkedSpecialBonusOperation = kvObject.GetStringOrDefault("LinkedSpecialBonusOperation", formatProvider: CultureInfo.InvariantCulture),
        };
    }
}
