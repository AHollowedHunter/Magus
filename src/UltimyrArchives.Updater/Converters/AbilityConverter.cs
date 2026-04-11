using Magus.Common.Dota.Enums;
using Magus.Common.Dota.ModelsV2;
using Magus.Common.Dota.ModelsV2.AbilityValue;
using Serilog;
using System.Diagnostics;
using UltimyrArchives.Updater.Extensions;
using ValveResourceFormat.Serialization.KeyValues;

namespace UltimyrArchives.Updater.Converters;

public sealed class AbilityConverter(KVObject baseAbility, KVObject abilityIds) : KVObjectConverter
{
    private readonly BaseAbilityValues _baseAbility = ConvertBaseAbility(baseAbility);
    private readonly Dictionary<string, int> _unitAbilityIds = ConvertAbilityIds(abilityIds, "UnitAbilities");
    private readonly Dictionary<string, int> _itemAbilityIds = ConvertAbilityIds(abilityIds, "ItemAbilities");

    public UnitAbility ConvertUnitAbility(KVOPair kvoPair)
    {
        (string name, KVObject ability) = kvoPair;
        var abilityType = ability.GetEnumOrDefault("AbilityType", _baseAbility.AbilityType);

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
            Id                    = _unitAbilityIds[name],
            AbilityValues         = ability.GetValueOrDefault("AbilityValues", []).Select(UnitAbilityValueConverter).ToArray(),
            AbilitySharedCooldown = ability.GetStringOrDefault("AbilitySharedCooldown", formatProvider: CultureInfo.InvariantCulture),
            MaxLevel              = maxLevel,

            // Enums
            AbilityType            = abilityType,
            AbilityBehavior        = ability.GetEnumOrDefault("AbilityBehavior", _baseAbility.AbilityBehavior),
            AbilityUnitDamageType  = ability.GetEnumOrDefault<AbilityUnitDamageType>("AbilityUnitDamageType"),
            AbilityUnitTargetTeam  = ability.GetEnumOrDefault<AbilityUnitTargetTeam>("AbilityUnitTargetTeam"),
            AbilityUnitTargetType  = ability.GetEnumOrDefault<AbilityUnitTargetType>("AbilityUnitTargetType"),
            AbilityUnitTargetFlags = ability.GetEnumOrDefault<AbilityUnitTargetFlags>("AbilityUnitTargetFlags"),
            SpellImmunityType      = ability.GetEnumOrDefault<SpellImmunityType>("SpellImmunityType"),
            SpellDispellableType   = ability.GetEnumOrDefault<SpellDispellableType>("SpellDispellableType"),

            // Stats
            AbilityCastRange          = ability.GetValueOrDefault("AbilityCastRange")?.ParseArray<float>() ?? _baseAbility.AbilityCastRange,
            AbilityOvershootCastRange = ability.GetValueOrDefault("AbilityOvershootCastRange")?.ParseArray<float>() ?? _baseAbility.AbilityOvershootCastRange,
            AbilityCastRangeBuffer    = ability.GetValueOrDefault("AbilityCastRangeBuffer")?.ParseArray<float>() ?? _baseAbility.AbilityCastRangeBuffer,
            AbilityCastPoint          = ability.GetValueOrDefault("AbilityCastPoint")?.ParseArray<float>() ?? _baseAbility.AbilityCastPoint,
            AbilityChannelTime        = ability.GetValueOrDefault("AbilityChannelTime")?.ParseArray<float>() ?? _baseAbility.AbilityChannelTime,
            AbilityCooldown           = ability.GetValueOrDefault("AbilityCooldown")?.ParseArray<float>() ?? _baseAbility.AbilityCooldown,
            AbilityDuration           = ability.GetValueOrDefault("AbilityDuration")?.ParseArray<float>() ?? _baseAbility.AbilityDuration,
            AbilityCharges            = ability.GetValueOrDefault("AbilityCharges")?.ParseArray<float>() ?? _baseAbility.AbilityCharges,
            AbilityChargeRestoreTime  = ability.GetValueOrDefault("AbilityChargeRestoreTime")?.ParseArray<float>() ?? _baseAbility.AbilityChargeRestoreTime,
            AbilityDamage             = ability.GetValueOrDefault("AbilityDamage")?.ParseArray<float>() ?? _baseAbility.AbilityDamage,
            AbilityManaCost           = ability.GetValueOrDefault("AbilityManaCost")?.ParseArray<float>() ?? _baseAbility.AbilityManaCost,
            AbilityHealthCost         = ability.GetValueOrDefault("AbilityHealthCost")?.ParseArray<float>() ?? _baseAbility.AbilityHealthCost,

            // Unit Ability
            IsBreakable        = ability.GetBooleanOrDefault("IsBreakable", formatProvider: CultureInfo.InvariantCulture),
            IsGrantedByScepter = ability.GetBooleanOrDefault("IsGrantedByScepter", formatProvider: CultureInfo.InvariantCulture),
            HasScepterUpgrade  = ability.GetBooleanOrDefault("HasScepterUpgrade", formatProvider: CultureInfo.InvariantCulture),
            IsGrantedByShard   = ability.GetBooleanOrDefault("IsGrantedByShard", formatProvider: CultureInfo.InvariantCulture),
            HasShardUpgrade    = ability.GetBooleanOrDefault("HasShardUpgrade", formatProvider: CultureInfo.InvariantCulture),
        };
    }

    public Item ConvertItem(KVOPair kvoPair)
    {
        (string name, KVObject item) = kvoPair;
        // TEST
        // if (item.GetValueOrDefault("AbilityCastRange") is { } abilityCastRange)
        // {
        //     var valueType = abilityCastRange.ValueType;
        //     Debugger.Break();
        // }

        // END TEST
        return new Item
        {
            InternalName          = name,
            Id                    = _itemAbilityIds[name],
            AbilityValues         = item.GetValueOrDefault("AbilityValues", []).Select(ItemAbilityValueConverter).ToArray(),
            AbilitySharedCooldown = item.GetStringOrDefault("AbilitySharedCooldown", formatProvider: CultureInfo.InvariantCulture),
            MaxLevel              = item.GetByteOrDefault("MaxLevel", 1, formatProvider: CultureInfo.InvariantCulture),

            // Enums
            AbilityType            = _baseAbility.AbilityType,
            AbilityBehavior        = item.GetEnumOrDefault("AbilityBehavior", _baseAbility.AbilityBehavior),
            AbilityUnitDamageType  = item.GetEnumOrDefault<AbilityUnitDamageType>("AbilityUnitDamageType"),
            AbilityUnitTargetTeam  = item.GetEnumOrDefault<AbilityUnitTargetTeam>("AbilityUnitTargetTeam"),
            AbilityUnitTargetType  = item.GetEnumOrDefault<AbilityUnitTargetType>("AbilityUnitTargetType"),
            AbilityUnitTargetFlags = item.GetEnumOrDefault<AbilityUnitTargetFlags>("AbilityUnitTargetFlags"),
            SpellImmunityType      = item.GetEnumOrDefault<SpellImmunityType>("SpellImmunityType"),
            SpellDispellableType   = item.GetEnumOrDefault<SpellDispellableType>("SpellDispellableType"),

            // Stats
            AbilityCastRange          = item.GetValueOrDefault("AbilityCastRange")?.ParseArray<float>() ?? _baseAbility.AbilityCastRange,
            AbilityOvershootCastRange = item.GetValueOrDefault("AbilityOvershootCastRange")?.ParseArray<float>() ?? _baseAbility.AbilityOvershootCastRange,
            AbilityCastRangeBuffer    = item.GetValueOrDefault("AbilityCastRangeBuffer")?.ParseArray<float>() ?? _baseAbility.AbilityCastRangeBuffer,
            AbilityCastPoint          = item.GetValueOrDefault("AbilityCastPoint")?.ParseArray<float>() ?? _baseAbility.AbilityCastPoint,
            AbilityChannelTime        = item.GetValueOrDefault("AbilityChannelTime")?.ParseArray<float>() ?? _baseAbility.AbilityChannelTime,
            AbilityCooldown           = item.GetValueOrDefault("AbilityCooldown")?.ParseArray<float>() ?? _baseAbility.AbilityCooldown,
            AbilityDuration           = item.GetValueOrDefault("AbilityDuration")?.ParseArray<float>() ?? _baseAbility.AbilityDuration,
            AbilityCharges            = item.GetValueOrDefault("AbilityCharges")?.ParseArray<float>() ?? _baseAbility.AbilityCharges,
            AbilityChargeRestoreTime  = item.GetValueOrDefault("AbilityChargeRestoreTime")?.ParseArray<float>() ?? _baseAbility.AbilityChargeRestoreTime,
            AbilityDamage             = item.GetValueOrDefault("AbilityDamage")?.ParseArray<float>() ?? _baseAbility.AbilityDamage,
            AbilityManaCost           = item.GetValueOrDefault("AbilityManaCost")?.ParseArray<float>() ?? _baseAbility.AbilityManaCost,
            AbilityHealthCost         = item.GetValueOrDefault("AbilityHealthCost")?.ParseArray<float>() ?? _baseAbility.AbilityHealthCost,

            // Item
            ItemAliases          = item.GetValueOrDefault("ItemAliases").ParseArray<string>(),
            ItemCost             = item.GetInt32OrDefault("ItemCost", _baseAbility.ItemCost, CultureInfo.InvariantCulture),
            ItemInitialCharges   = item.GetInt32OrDefault("ItemInitialCharges", _baseAbility.ItemInitialCharges, CultureInfo.InvariantCulture),
            ItemRequiresCharges  = item.GetBooleanOrDefault("ItemRequiresCharges", _baseAbility.ItemRequiresCharges, CultureInfo.InvariantCulture),
            ItemStockInitial     = item.GetInt32OrDefault("ItemStockInitial", formatProvider: CultureInfo.InvariantCulture),
            ItemStockMax         = item.GetInt32OrDefault("ItemStockMax", formatProvider: CultureInfo.InvariantCulture),
            ItemStockTime        = item.GetInt32OrDefault("ItemStockTime", formatProvider: CultureInfo.InvariantCulture),
            ItemInitialStockTime = item.GetInt32OrDefault("ItemInitialStockTime", formatProvider: CultureInfo.InvariantCulture),
            ItemIsNeutralDrop    = item.GetBooleanOrDefault("ItemIsNeutralDrop", formatProvider: CultureInfo.InvariantCulture),
            MaxUpgradeLevel      = item.GetByteOrDefault("MaxUpgradeLevel", formatProvider: CultureInfo.InvariantCulture),
            ItemBaseLevel        = item.GetByteOrDefault("ItemBaseLevel", formatProvider: CultureInfo.InvariantCulture),
            ItemDroppable        = item.GetBooleanOrDefault("ItemDroppable", _baseAbility.ItemDroppable, CultureInfo.InvariantCulture),
            ItemPurchasable      = item.GetBooleanOrDefault("ItemPurchasable", _baseAbility.ItemPurchasable, CultureInfo.InvariantCulture),
            ItemSellable         = item.GetBooleanOrDefault("ItemSellable", _baseAbility.ItemSellable, CultureInfo.InvariantCulture),
            IsObsolete           = item.GetBooleanOrDefault("IsObsolete", false, CultureInfo.InvariantCulture),
            ItemRecipe           = item.GetBooleanOrDefault("ItemRecipe", _baseAbility.ItemRecipe, CultureInfo.InvariantCulture),
            ItemResult           = item.GetStringOrDefault("ItemResult", formatProvider: CultureInfo.InvariantCulture),
            ItemRequirements     = item.GetValueOrDefault("ItemRequirements") is { } value ? ItemRequirementConverter(value) : null,
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

    private static IAbilityValue ItemAbilityValueConverter(KVOPair kvoPair)
    {
        return kvoPair.Value.Count is 0
            ? new BasicValue(
                kvoPair.Key,
                kvoPair.Value.ParseArray<float>())
            : new BasicValue(
                kvoPair.Key,
                kvoPair.Value["value"].ParseArray<float>(),
                kvoPair.Value.GetBooleanOrDefault("affected_by_aoe_increase"));
    }

    private static Item.ItemRequirement[][] ItemRequirementConverter(KVObject kvObject)
    {
        var allRequirements = new Item.ItemRequirement[kvObject.Count][];
        for (var childIndex = 0; childIndex < allRequirements.Length; childIndex++)
        {
            var items = kvObject[childIndex]
                .ToString(CultureInfo.InvariantCulture)
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var requirements = new Item.ItemRequirement[items.Length];
            for (var i = 0; i < items.Length; i++)
                requirements[i] = items[i][^1] == '*'
                    ? new Item.ItemRequirement(items[i][..^1], true)
                    : new Item.ItemRequirement(items[i]);
            allRequirements[childIndex] = requirements;
        }

        return allRequirements;
    }

    private static Dictionary<string, int> ConvertAbilityIds(KVObject kvAbilityIds, string groupKey)
    {
        Dictionary<string, int> abilityIds = [];
        foreach ((string name, KVObject ability) in kvAbilityIds[groupKey]["Locked"])
        {
            var abilityId = ability.ToInt32(CultureInfo.InvariantCulture);
            if (abilityIds.TryAdd(name, ability.ToInt32(CultureInfo.InvariantCulture)) is false)
                // TODO improve logging/handling
                Log.Warning(
                    "Possible duplicate ability_id for {name}, tried adding {newId} alongside {existingId}",
                    name,
                    abilityId,
                    abilityIds[name]);
        }

        return abilityIds;
    }

    private static BaseAbilityValues ConvertBaseAbility(KVObject baseAbility) => new()
    {
        AbilityType               = baseAbility["AbilityType"].ToEnum<AbilityType>(),
        AbilityBehavior           = baseAbility["AbilityBehavior"].ToEnum<AbilityBehavior>(),
        AbilityCastRange          = baseAbility.GetValueOrDefault("AbilityCastRange").ParseArray<float>(),
        AbilityOvershootCastRange = baseAbility.GetValueOrDefault("AbilityOvershootCastRange").ParseArray<float>(),
        AbilityCastRangeBuffer    = baseAbility.GetValueOrDefault("AbilityCastRangeBuffer").ParseArray<float>(),
        AbilityCastPoint          = baseAbility.GetValueOrDefault("AbilityCastPoint").ParseArray<float>(),
        AbilityChannelTime        = baseAbility.GetValueOrDefault("AbilityChannelTime").ParseArray<float>(),
        AbilityCooldown           = baseAbility.GetValueOrDefault("AbilityCooldown").ParseArray<float>(),
        AbilityDuration           = baseAbility.GetValueOrDefault("AbilityDuration").ParseArray<float>(),
        AbilityCharges            = baseAbility.GetValueOrDefault("AbilityCharges").ParseArray<float>(),
        AbilityChargeRestoreTime  = baseAbility.GetValueOrDefault("AbilityChargeRestoreTime").ParseArray<float>(),
        AbilityDamage             = baseAbility.GetValueOrDefault("AbilityDamage").ParseArray<float>(),
        AbilityManaCost           = baseAbility.GetValueOrDefault("AbilityManaCost").ParseArray<float>(),
        AbilityHealthCost         = [0], // Not set in base ability.
        // Item
        ItemCost            = baseAbility.GetInt32OrDefault("ItemCost", formatProvider: CultureInfo.InvariantCulture),
        ItemInitialCharges  = baseAbility.GetInt32OrDefault("ItemInitialCharges", formatProvider: CultureInfo.InvariantCulture),
        ItemCombinable      = baseAbility.GetBooleanOrDefault("ItemCombinable", formatProvider: CultureInfo.InvariantCulture),
        ItemPermanent       = baseAbility.GetBooleanOrDefault("ItemPermanent", formatProvider: CultureInfo.InvariantCulture),
        ItemStackable       = baseAbility.GetBooleanOrDefault("ItemStackable", formatProvider: CultureInfo.InvariantCulture),
        ItemRecipe          = baseAbility.GetBooleanOrDefault("ItemRecipe", formatProvider: CultureInfo.InvariantCulture),
        ItemDroppable       = baseAbility.GetBooleanOrDefault("ItemDroppable", formatProvider: CultureInfo.InvariantCulture),
        ItemPurchasable     = baseAbility.GetBooleanOrDefault("ItemPurchasable", formatProvider: CultureInfo.InvariantCulture),
        ItemSellable        = baseAbility.GetBooleanOrDefault("ItemSellable", formatProvider: CultureInfo.InvariantCulture),
        ItemRequiresCharges = baseAbility.GetBooleanOrDefault("ItemRequiresCharges", formatProvider: CultureInfo.InvariantCulture),
        ItemDisassemblable  = baseAbility.GetBooleanOrDefault("ItemDisassemblable", formatProvider: CultureInfo.InvariantCulture),
        ItemIsNeutralDrop   = baseAbility.GetBooleanOrDefault("ItemIsNeutralDrop", formatProvider: CultureInfo.InvariantCulture),
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
        public required float[]         AbilityHealthCost         { get; init; }

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
