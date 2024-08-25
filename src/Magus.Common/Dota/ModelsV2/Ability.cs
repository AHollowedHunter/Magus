using Magus.Common.Dota.Enums;
using Magus.Common.Dota.ModelsV2.AbilityValue;

namespace Magus.Common.Dota.ModelsV2;

/// <summary>
/// Common Properties shared across all abilities.
/// </summary>
public abstract class Ability
{
    // Info
    public required string InternalName { get; set; }

    public int Id { get; set; }

    public AbilityType AbilityType { get; set; }

    public AbilityBehavior AbilityBehavior { get; set; }

    public AbilityUnitTargetTeam AbilityUnitTargetTeam { get; set; }

    public AbilityUnitTargetType AbilityUnitTargetType { get; set; }

    public AbilityUnitTargetFlags AbilityUnitTargetFlags { get; set; }

    public AbilityUnitDamageType AbilityUnitDamageType { get; set; }

    public SpellImmunityType SpellImmunityType { get; set; }

    public SpellDispellableType SpellDispellableType { get; set; }

    public string? AbilitySharedCooldown { get; set; }

    // Stats
    public required float[] AbilityCastRange { get; set; }

    public required float[] AbilityOvershootCastRange { get; set; }

    public required float[] AbilityCastRangeBuffer { get; set; }

    public required float[] AbilityCastPoint { get; set; }

    public required float[] AbilityChannelTime { get; set; }

    public required float[] AbilityCharges { get; set; }

    public required float[] AbilityChargeRestoreTime { get; set; }

    public required float[] AbilityCooldown { get; set; }

    public required float[] AbilityDuration { get; set; }

    public required float[] AbilityDamage { get; set; }

    public required float[] AbilityManaCost { get; set; }

    public required float[] AbilityHealthCost { get; set; }

    public required IAbilityValue[] AbilityValues { get; set; }

    public List<string> GetTargetTypeNames() // TODO localise see DOTA_ToolTip_Ability etc
    {
        var targetNames = new List<string>();
        if (AbilityBehavior.HasFlag(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_PASSIVE)) targetNames.Add("Passive");
        if (AbilityBehavior.HasFlag(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_NO_TARGET)) targetNames.Add("No Target"); // e.g. DOTA_ToolTip_Ability_NoTarget
        if (AbilityBehavior.HasFlag(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_UNIT_TARGET)) targetNames.Add("Unit Target");
        if (AbilityBehavior.HasFlag(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_POINT)) targetNames.Add("Point Target");
        return targetNames;
    }
}
