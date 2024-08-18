using Magus.Common.Dota.Enums;
using Magus.Common.Dota.ModelsV2;
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
            MaxLevel              = maxLevel,
            AbilityValues         = [], // TODO
            AbilitySharedCooldown = kvAbility["AbilitySharedCooldown"]?.ToString(CultureInfo.InvariantCulture),
            IsBreakable           = kvAbility["IsBreakable"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,

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
            IsGrantedByScepter = kvAbility["IsGrantedByScepter"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
            HasScepterUpgrade  = kvAbility["HasScepterUpgrade"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
            IsGrantedByShard   = kvAbility["IsGrantedByShard"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
            HasShardUpgrade    = kvAbility["HasShardUpgrade"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
        };
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
        AbilityManaCost           = baseAbility.GetRequiredArray<float>("AbilityManaCost")
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
