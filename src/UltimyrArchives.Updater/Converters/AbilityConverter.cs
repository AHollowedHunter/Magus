using Magus.Common.Dota.Enums;
using Magus.Common.Dota.ModelsV2;
using Magus.Common.Dota.ModelsV2.AbilityValue;
using System.Globalization;
using UltimyrArchives.Updater.Extensions;
using ValveKeyValue;

namespace UltimyrArchives.Updater.Converters;

public sealed class AbilityConverter(KVObject baseAbility, KVObject abilityIds) : KVObjectConverter
{
    private readonly BaseAbilityValues _baseAbility = ConvertBaseAbility(baseAbility);
    private readonly Dictionary<string, int> _unitAbilityIds = ConvertAbilityIds(abilityIds, "UnitAbilities");
    private readonly Dictionary<string, int> _itemAbilityIds = ConvertAbilityIds(abilityIds, "ItemAbilities");

    public UnitAbility ConvertUnitAbility(KVObject kvAbility)
    {
        var abilityType = kvAbility["AbilityType"]?.ToEnum<AbilityType>() ?? _baseAbility.AbilityType;
        var maxLevel    = kvAbility["MaxLevel"]?.ToByte(CultureInfo.InvariantCulture) ?? (byte) (abilityType is AbilityType.DOTA_ABILITY_TYPE_ULTIMATE ? 3 : 4);
        return new UnitAbility
        {
            InternalName          = kvAbility.Name,
            Id                    = _unitAbilityIds[kvAbility.Name],
            AbilityValues         = ConvertList(kvAbility["AbilityValues"], HeroAbilityValueConverter),
            AbilitySharedCooldown = kvAbility["AbilitySharedCooldown"]?.ToString(CultureInfo.InvariantCulture),

            // Enums
            AbilityType            = abilityType,
            AbilityBehavior        = kvAbility["AbilityBehavior"]?.ToEnum<AbilityBehavior>() ?? _baseAbility.AbilityBehavior,
            AbilityUnitDamageType  = kvAbility["AbilityUnitDamageType"].ToEnum<AbilityUnitDamageType>(),
            AbilityUnitTargetTeam  = kvAbility["AbilityUnitTargetTeam"].ToEnum<AbilityUnitTargetTeam>(),
            AbilityUnitTargetType  = kvAbility["AbilityUnitTargetType"].ToEnum<AbilityUnitTargetType>(),
            AbilityUnitTargetFlags = kvAbility["AbilityUnitTargetFlags"].ToEnum<AbilityUnitTargetFlags>(),
            SpellImmunityType      = kvAbility["SpellImmunityType"].ToEnum<SpellImmunityType>(),
            SpellDispellableType   = kvAbility["SpellDispellableType"].ToEnum<SpellDispellableType>(),

            // Stats
            AbilityCastRange          = kvAbility["AbilityCastRange"]?.ParseArray<float>() ?? _baseAbility.AbilityCastRange,
            AbilityOvershootCastRange = kvAbility["AbilityOvershootCastRange"]?.ParseArray<float>() ?? _baseAbility.AbilityOvershootCastRange,
            AbilityCastRangeBuffer    = kvAbility["AbilityCastRangeBuffer"]?.ParseArray<float>() ?? _baseAbility.AbilityCastRangeBuffer,
            AbilityCastPoint          = kvAbility["AbilityCastPoint"]?.ParseArray<float>() ?? _baseAbility.AbilityCastPoint,
            AbilityChannelTime        = kvAbility["AbilityChannelTime"]?.ParseArray<float>() ?? _baseAbility.AbilityChannelTime,
            AbilityCooldown           = kvAbility["AbilityCooldown"]?.ParseArray<float>() ?? _baseAbility.AbilityCooldown,
            AbilityDuration           = kvAbility["AbilityDuration"]?.ParseArray<float>() ?? _baseAbility.AbilityDuration,
            AbilityCharges            = kvAbility["AbilityCharges"]?.ParseArray<float>() ?? _baseAbility.AbilityCharges,
            AbilityChargeRestoreTime  = kvAbility["AbilityChargeRestoreTime"]?.ParseArray<float>() ?? _baseAbility.AbilityChargeRestoreTime,
            AbilityDamage             = kvAbility["AbilityDamage"]?.ParseArray<float>() ?? _baseAbility.AbilityDamage,
            AbilityManaCost           = kvAbility["AbilityManaCost"]?.ParseArray<float>() ?? _baseAbility.AbilityManaCost,
            AbilityHealthCost         = kvAbility["AbilityHealthCost"]?.ParseArray<float>() ?? _baseAbility.AbilityHealthCost,

            // Unit Ability
            MaxLevel           = maxLevel,
            IsBreakable        = kvAbility["IsBreakable"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
            IsGrantedByScepter = kvAbility["IsGrantedByScepter"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
            HasScepterUpgrade  = kvAbility["HasScepterUpgrade"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
            IsGrantedByShard   = kvAbility["IsGrantedByShard"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
            HasShardUpgrade    = kvAbility["HasShardUpgrade"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
        };
    }

    public Item ConvertItem(KVObject item) => new()
    {
        InternalName          = item.Name,
        Id                    = _itemAbilityIds[item.Name],
        AbilityValues         = ConvertList(item["AbilityValues"], ItemAbilityValueConverter),
        AbilitySharedCooldown = item["AbilitySharedCooldown"]?.ToString(CultureInfo.InvariantCulture),

        // Enums
        AbilityType            = _baseAbility.AbilityType,
        AbilityBehavior        = item["AbilityBehavior"]?.ToEnum<AbilityBehavior>() ?? _baseAbility.AbilityBehavior,
        AbilityUnitDamageType  = item["AbilityUnitDamageType"].ToEnum<AbilityUnitDamageType>(),
        AbilityUnitTargetTeam  = item["AbilityUnitTargetTeam"].ToEnum<AbilityUnitTargetTeam>(),
        AbilityUnitTargetType  = item["AbilityUnitTargetType"].ToEnum<AbilityUnitTargetType>(),
        AbilityUnitTargetFlags = item["AbilityUnitTargetFlags"].ToEnum<AbilityUnitTargetFlags>(),
        SpellImmunityType      = item["SpellImmunityType"].ToEnum<SpellImmunityType>(),
        SpellDispellableType   = item["SpellDispellableType"].ToEnum<SpellDispellableType>(),

        // Stats
        AbilityCastRange          = item["AbilityCastRange"]?.ParseArray<float>() ?? _baseAbility.AbilityCastRange,
        AbilityOvershootCastRange = item["AbilityOvershootCastRange"]?.ParseArray<float>() ?? _baseAbility.AbilityOvershootCastRange,
        AbilityCastRangeBuffer    = item["AbilityCastRangeBuffer"]?.ParseArray<float>() ?? _baseAbility.AbilityCastRangeBuffer,
        AbilityCastPoint          = item["AbilityCastPoint"]?.ParseArray<float>() ?? _baseAbility.AbilityCastPoint,
        AbilityChannelTime        = item["AbilityChannelTime"]?.ParseArray<float>() ?? _baseAbility.AbilityChannelTime,
        AbilityCooldown           = item["AbilityCooldown"]?.ParseArray<float>() ?? _baseAbility.AbilityCooldown,
        AbilityDuration           = item["AbilityDuration"]?.ParseArray<float>() ?? _baseAbility.AbilityDuration,
        AbilityCharges            = item["AbilityCharges"]?.ParseArray<float>() ?? _baseAbility.AbilityCharges,
        AbilityChargeRestoreTime  = item["AbilityChargeRestoreTime"]?.ParseArray<float>() ?? _baseAbility.AbilityChargeRestoreTime,
        AbilityDamage             = item["AbilityDamage"]?.ParseArray<float>() ?? _baseAbility.AbilityDamage,
        AbilityManaCost           = item["AbilityManaCost"]?.ParseArray<float>() ?? _baseAbility.AbilityManaCost,
        AbilityHealthCost         = item["AbilityHealthCost"]?.ParseArray<float>() ?? _baseAbility.AbilityHealthCost,

        // Item
        ItemAliases          = item["ItemAliases"].ParseArray<string>(),
        ItemCost             = item["ItemCost"]?.ToInt32(CultureInfo.InvariantCulture) ?? _baseAbility.ItemCost,
        ItemInitialCharges   = item["ItemInitialCharges"]?.ToInt32(CultureInfo.InvariantCulture) ?? _baseAbility.ItemInitialCharges,
        ItemRequiresCharges  = item["ItemRequiresCharges"]?.ToBoolean(CultureInfo.InvariantCulture) ?? _baseAbility.ItemRequiresCharges,
        ItemStockInitial     = item["ItemStockInitial"]?.ToInt32(CultureInfo.InvariantCulture),
        ItemStockMax         = item["ItemStockMax"]?.ToInt32(CultureInfo.InvariantCulture),
        ItemStockTime        = item["ItemStockTime"]?.ToInt32(CultureInfo.InvariantCulture),
        ItemInitialStockTime = item["ItemInitialStockTime"]?.ToInt32(CultureInfo.InvariantCulture),
        ItemIsNeutralDrop    = item["ItemIsNeutralDrop"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
        MaxUpgradeLevel      = item["MaxUpgradeLevel"]?.ToByte(CultureInfo.InvariantCulture),
        ItemBaseLevel        = item["ItemBaseLevel"]?.ToByte(CultureInfo.InvariantCulture),
        ItemDroppable        = item["ItemDroppable"]?.ToBoolean(CultureInfo.InvariantCulture) ?? _baseAbility.ItemDroppable,
        ItemPurchasable      = item["ItemPurchasable"]?.ToBoolean(CultureInfo.InvariantCulture) ?? _baseAbility.ItemPurchasable,
        ItemSellable         = item["ItemSellable"]?.ToBoolean(CultureInfo.InvariantCulture) ?? _baseAbility.ItemSellable,
        IsObsolete           = item["IsObsolete"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
        ItemRecipe           = item["ItemRecipe"]?.ToBoolean(CultureInfo.InvariantCulture) ?? _baseAbility.ItemRecipe,
        ItemResult           = item["ItemResult"]?.ToString(CultureInfo.InvariantCulture),
        // Want null, not an empty array. Cast as enumerable here first to filter out empty values...
        ItemRequirements = item["ItemRequirements"].AsEnumerable() is { } value ? ConvertList([..value], ItemRequirementConverter) : null,
    };

    private static IAbilityValue HeroAbilityValueConverter(KVObject kvObject)
    {
        if (kvObject.Value is not IEnumerable<KVObject>)
            return new BasicValue(kvObject.Name, kvObject.Value.ParseArray<float>());

        var values = kvObject.Children.ToArray();
        switch (values.Length)
        {
            case 0:
                return new BasicValue(kvObject.Name, []); // only seems to happen with commented out values
            case <= 2 when values.All(x => BasicValue.Keys.Contains(x.Name)):
                return new BasicValue(
                    kvObject.Name,
                    kvObject.GetRequiredArray<float>("value"),
                    kvObject["affected_by_aoe_increase"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false);
        }

        SpecialBonus[]? specialBonus = null;
        if (values.Where(x => Rx.SpecialBonus.IsMatch(x.Name)) is { } specialBonuses)
            specialBonus =
            [
                ..specialBonuses.Select(x => new SpecialBonus(x.Name, SpecialBonusValue.Parse(x.Value.ToString(CultureInfo.InvariantCulture).Split())))
            ];
        return new HeroesAbilityValue
        {
            Name                        = kvObject.Name,
            Value                       = kvObject["value"]?.ParseArray<float>() ?? [],
            AffectedByAOEIncrease       = kvObject["affected_by_aoe_increase"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
            SpecialBonuses              = specialBonus,
            Innate                      = kvObject["Innate"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
            RequiresFacet               = kvObject["RequiresFacet"]?.ToString(CultureInfo.InvariantCulture),
            RequiresScepter             = kvObject["RequiresScepter"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
            RequiresShard               = kvObject["RequiresShard"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
            LinkedSpecialBonus          = kvObject["LinkedSpecialBonus"]?.ToString(CultureInfo.InvariantCulture),
            LinkedSpecialBonusField     = kvObject["LinkedSpecialBonusField"]?.ToString(CultureInfo.InvariantCulture),
            LinkedSpecialBonusOperation = kvObject["LinkedSpecialBonusOperation"]?.ToString(CultureInfo.InvariantCulture)
        };
    }

    private static IAbilityValue ItemAbilityValueConverter(KVObject kvObject)
    {
        return kvObject.Value is not IEnumerable<KVObject>
            ? new BasicValue(kvObject.Name, kvObject.Value.ParseArray<float>())
            : new BasicValue(kvObject.Name, kvObject.GetRequiredArray<float>("value"), kvObject.GetRequiredBoolean("affected_by_aoe_increase"));
    }

    private static Item.ItemRequirement[] ItemRequirementConverter(KVObject kvObject)
    {
        var items = kvObject.Value.ToString(CultureInfo.InvariantCulture).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var requirements = new Item.ItemRequirement[items.Length];
        for (var i = 0; i < items.Length; i++)
            requirements[i] = items[i][^1] == '*'
                ? new Item.ItemRequirement(items[i][..^1], true)
                : new Item.ItemRequirement(items[i]);
        return requirements;
    }

    private static Dictionary<string, int> ConvertAbilityIds(KVObject abilityIds, string groupKey)
        => abilityIds[groupKey]["Locked"]
            .CastEnumerable()
            .ToDictionary(x => x.Name, x => x.Value.ToInt32(CultureInfo.InvariantCulture));

    private static BaseAbilityValues ConvertBaseAbility(KVObject baseAbility) => new()
    {
        AbilityType               = baseAbility.GetRequiredEnum<AbilityType>("AbilityType"),
        AbilityBehavior           = baseAbility.GetRequiredEnum<AbilityBehavior>("AbilityBehavior"),
        AbilityCastRange          = baseAbility.GetRequiredArray<float>("AbilityCastRange"),
        AbilityOvershootCastRange = baseAbility.GetRequiredArray<float>("AbilityOvershootCastRange"),
        AbilityCastRangeBuffer    = baseAbility.GetRequiredArray<float>("AbilityCastRangeBuffer"),
        AbilityCastPoint          = baseAbility.GetRequiredArray<float>("AbilityCastPoint"),
        AbilityChannelTime        = baseAbility.GetRequiredArray<float>("AbilityChannelTime"),
        AbilityCooldown           = baseAbility.GetRequiredArray<float>("AbilityCooldown"),
        AbilityDuration           = baseAbility.GetRequiredArray<float>("AbilityDuration"),
        AbilityCharges            = baseAbility.GetRequiredArray<float>("AbilityCharges"),
        AbilityChargeRestoreTime  = baseAbility.GetRequiredArray<float>("AbilityChargeRestoreTime"),
        AbilityDamage             = baseAbility.GetRequiredArray<float>("AbilityDamage"),
        AbilityManaCost           = baseAbility.GetRequiredArray<float>("AbilityManaCost"),
        // Item
        ItemCost            = baseAbility.GetRequiredInt32("ItemCost"),
        ItemInitialCharges  = baseAbility.GetRequiredInt32("ItemInitialCharges"),
        ItemCombinable      = baseAbility.GetRequiredBoolean("ItemCombinable"),
        ItemPermanent       = baseAbility.GetRequiredBoolean("ItemPermanent"),
        ItemStackable       = baseAbility.GetRequiredBoolean("ItemStackable"),
        ItemRecipe          = baseAbility.GetRequiredBoolean("ItemRecipe"),
        ItemDroppable       = baseAbility.GetRequiredBoolean("ItemDroppable"),
        ItemPurchasable     = baseAbility.GetRequiredBoolean("ItemPurchasable"),
        ItemSellable        = baseAbility.GetRequiredBoolean("ItemSellable"),
        ItemRequiresCharges = baseAbility.GetRequiredBoolean("ItemRequiresCharges"),
        ItemDisassemblable  = baseAbility.GetRequiredBoolean("ItemDisassemblable"),
        ItemIsNeutralDrop   = baseAbility.GetRequiredBoolean("ItemIsNeutralDrop"),
    };

    private record BaseAbilityValues
    {
        public          AbilityType     AbilityType               { get; init; }
        public          AbilityBehavior AbilityBehavior           { get; init; }
        public required float[]         AbilityCastRange          { get; init; }
        public required float[]         AbilityOvershootCastRange { get; init; }
        public required float[]         AbilityCastRangeBuffer    { get; init; }
        public required float[]         AbilityCastPoint          { get; init; }
        public required float[]         AbilityChannelTime        { get; init; }
        public required float[]         AbilityCooldown           { get; init; }
        public required float[]         AbilityDuration           { get; init; }
        public required float[]         AbilityCharges            { get; init; }
        public required float[]         AbilityChargeRestoreTime  { get; init; }
        public required float[]         AbilityDamage             { get; init; }
        public required float[]         AbilityManaCost           { get; init; }
        public          float[]         AbilityHealthCost         { get; } = [0]; // Not set in base ability.

        // Item specific
        public int  ItemCost            { get; init; }
        public int  ItemInitialCharges  { get; init; }
        public bool ItemCombinable      { get; init; }
        public bool ItemPermanent       { get; init; }
        public bool ItemStackable       { get; init; }
        public bool ItemRecipe          { get; init; }
        public bool ItemDroppable       { get; init; }
        public bool ItemPurchasable     { get; init; }
        public bool ItemSellable        { get; init; }
        public bool ItemRequiresCharges { get; init; }
        public bool ItemDisassemblable  { get; init; }
        public bool ItemIsNeutralDrop   { get; init; }
    }
}
