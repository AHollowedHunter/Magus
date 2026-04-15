using Magus.Common.Dota.Enums;
using Magus.Common.Dota.ModelsV2;
using Magus.Common.Dota.ModelsV2.AbilityValue;
using UltimyrArchives.Updater.Extensions;

namespace UltimyrArchives.Updater.Converters;

public sealed class ItemConverter(KVObject baseAbility, Dictionary<string, int> itemAbilityIds) : AbilityConverter<Item>(baseAbility)
{
    public override Item Convert(string name, KVObject item)
    {
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
            Id                    = itemAbilityIds[name],
            AbilityValues         = item.GetValueOrDefault("AbilityValues", []).Select(ItemAbilityValueConverter).ToArray(),
            AbilitySharedCooldown = item.GetStringOrDefault("AbilitySharedCooldown", formatProvider: CultureInfo.InvariantCulture),
            MaxLevel              = item.GetByteOrDefault("MaxLevel", 1, formatProvider: CultureInfo.InvariantCulture),

            // Enums
            AbilityType            = BaseAbility.AbilityType,
            AbilityBehavior        = item.GetEnumOrDefault("AbilityBehavior", BaseAbility.AbilityBehavior),
            AbilityUnitDamageType  = item.GetEnumOrDefault<AbilityUnitDamageType>("AbilityUnitDamageType"),
            AbilityUnitTargetTeam  = item.GetEnumOrDefault<AbilityUnitTargetTeam>("AbilityUnitTargetTeam"),
            AbilityUnitTargetType  = item.GetEnumOrDefault<AbilityUnitTargetType>("AbilityUnitTargetType"),
            AbilityUnitTargetFlags = item.GetEnumOrDefault<AbilityUnitTargetFlags>("AbilityUnitTargetFlags"),
            SpellImmunityType      = item.GetEnumOrDefault<SpellImmunityType>("SpellImmunityType"),
            SpellDispellableType   = item.GetEnumOrDefault<SpellDispellableType>("SpellDispellableType"),

            // Stats
            AbilityCastRange          = item.GetValueOrDefault("AbilityCastRange")?.ParseArray<float>() ?? BaseAbility.AbilityCastRange,
            AbilityOvershootCastRange = item.GetValueOrDefault("AbilityOvershootCastRange")?.ParseArray<float>() ?? BaseAbility.AbilityOvershootCastRange,
            AbilityCastRangeBuffer    = item.GetValueOrDefault("AbilityCastRangeBuffer")?.ParseArray<float>() ?? BaseAbility.AbilityCastRangeBuffer,
            AbilityCastPoint          = item.GetValueOrDefault("AbilityCastPoint")?.ParseArray<float>() ?? BaseAbility.AbilityCastPoint,
            AbilityChannelTime        = item.GetValueOrDefault("AbilityChannelTime")?.ParseArray<float>() ?? BaseAbility.AbilityChannelTime,
            AbilityCooldown           = item.GetValueOrDefault("AbilityCooldown")?.ParseArray<float>() ?? BaseAbility.AbilityCooldown,
            AbilityDuration           = item.GetValueOrDefault("AbilityDuration")?.ParseArray<float>() ?? BaseAbility.AbilityDuration,
            AbilityCharges            = item.GetValueOrDefault("AbilityCharges")?.ParseArray<float>() ?? BaseAbility.AbilityCharges,
            AbilityChargeRestoreTime  = item.GetValueOrDefault("AbilityChargeRestoreTime")?.ParseArray<float>() ?? BaseAbility.AbilityChargeRestoreTime,
            AbilityDamage             = item.GetValueOrDefault("AbilityDamage")?.ParseArray<float>() ?? BaseAbility.AbilityDamage,
            AbilityManaCost           = item.GetValueOrDefault("AbilityManaCost")?.ParseArray<float>() ?? BaseAbility.AbilityManaCost,
            AbilityHealthCost         = item.GetValueOrDefault("AbilityHealthCost")?.ParseArray<float>() ?? BaseAbility.AbilityHealthCost,

            // Item
            ItemAliases          = item.GetValueOrDefault("ItemAliases").ParseArray<string>(),
            ItemCost             = item.GetInt32OrDefault("ItemCost", BaseAbility.ItemCost, CultureInfo.InvariantCulture),
            ItemInitialCharges   = item.GetInt32OrDefault("ItemInitialCharges", BaseAbility.ItemInitialCharges, CultureInfo.InvariantCulture),
            ItemRequiresCharges  = item.GetBooleanOrDefault("ItemRequiresCharges", BaseAbility.ItemRequiresCharges, CultureInfo.InvariantCulture),
            ItemStockInitial     = item.GetInt32OrDefault("ItemStockInitial", formatProvider: CultureInfo.InvariantCulture),
            ItemStockMax         = item.GetInt32OrDefault("ItemStockMax", formatProvider: CultureInfo.InvariantCulture),
            ItemStockTime        = item.GetInt32OrDefault("ItemStockTime", formatProvider: CultureInfo.InvariantCulture),
            ItemInitialStockTime = item.GetInt32OrDefault("ItemInitialStockTime", formatProvider: CultureInfo.InvariantCulture),
            ItemIsNeutralDrop    = item.GetBooleanOrDefault("ItemIsNeutralDrop", formatProvider: CultureInfo.InvariantCulture),
            MaxUpgradeLevel      = item.GetByteOrDefault("MaxUpgradeLevel", formatProvider: CultureInfo.InvariantCulture),
            ItemBaseLevel        = item.GetByteOrDefault("ItemBaseLevel", formatProvider: CultureInfo.InvariantCulture),
            ItemDroppable        = item.GetBooleanOrDefault("ItemDroppable", BaseAbility.ItemDroppable, CultureInfo.InvariantCulture),
            ItemPurchasable      = item.GetBooleanOrDefault("ItemPurchasable", BaseAbility.ItemPurchasable, CultureInfo.InvariantCulture),
            ItemSellable         = item.GetBooleanOrDefault("ItemSellable", BaseAbility.ItemSellable, CultureInfo.InvariantCulture),
            IsObsolete           = item.GetBooleanOrDefault("IsObsolete", false, CultureInfo.InvariantCulture),
            ItemRecipe           = item.GetBooleanOrDefault("ItemRecipe", BaseAbility.ItemRecipe, CultureInfo.InvariantCulture),
            ItemResult           = item.GetStringOrDefault("ItemResult", formatProvider: CultureInfo.InvariantCulture),
            ItemRequirements     = item.GetValueOrDefault("ItemRequirements") is { } value ? ItemRequirementConverter(value) : null,
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

}
