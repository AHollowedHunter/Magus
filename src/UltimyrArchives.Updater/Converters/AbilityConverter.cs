using Magus.Common.Dota.Enums;
using Serilog;
using UltimyrArchives.Updater.Extensions;

namespace UltimyrArchives.Updater.Converters;

public abstract class AbilityConverter<TEntity>(KVObject baseAbility) : IKVObjectConverter<TEntity>
{
    protected readonly BaseAbilityValues BaseAbility = ConvertBaseAbility(baseAbility);
    
    public abstract TEntity Convert(string name, KVObject ability);

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

    protected record BaseAbilityValues
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
