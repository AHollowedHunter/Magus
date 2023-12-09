using Magus.Common.Dota.Enums;

namespace Magus.Common.Dota.Models;

/// <summary>
/// DotA appears to handle hero abilities & talents along with items as the same identical "ability".
/// Have them all inherit from a base class, which I decided to call "spells"
/// </summary>
public abstract record BaseSpell
{
    // Info
    public int Id { get; set; }
    public string Language { get; set; }
    public string Name { get; set; }
    public string InternalName { get; set; }
    public string Description { get; set; } // Move to ability, and change for Items, which have multiple actives, so List<string> split via <h1> tags
    public string Lore { get; set; }
    public IList<string> Notes { get; set; }

    public byte MaxLevel { get; set; }
    public AbilityType AbilityType { get; set; }
    public AbilityBehavior AbilityBehavior { get; set; }
    public AbilityUnitTargetTeam AbilityUnitTargetTeam { get; set; }
    public AbilityUnitTargetType AbilityUnitTargetType { get; set; }
    public AbilityUnitDamageType AbilityUnitDamageType { get; set; }
    public SpellImmunityType SpellImmunityType { get; set; }
    public SpellDispellableType SpellDispellableType { get; set; }

    public int Flags { get; set; } // What is this?

    // Stats
    public IList<float> AbilityCastRange { get; set; }
    public IList<float> AbilityCastPoint { get; set; }
    public IList<float> AbilityChannelTime { get; set; }
    public IList<float> AbilityCharges { get; set; }
    public IList<float> AbilityChargeRestoreTime { get; set; }
    public IList<float> AbilityCooldown { get; set; }
    public IList<float> AbilityDuration { get; set; }
    public IList<float> AbilityDamage { get; set; }
    public IList<float> AbilityManaCost { get; set; }
    public IList<float> AbilityHealthCost { get; set; }

    public IList<AbilityValue> AbilityValues { get; set; }
    public IDictionary<string, string> DisplayedValues { get; set; }
    public record AbilityValue
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public IList<float> Values { get; set; }
        public string? LinkedSpecialBonus { get; set; }
        public string? SpecialBonusValue { get; set; }
    }

    // Is there anything we don't know?
    public Dictionary<string, object>? ExtensionData { get; set; }

    public bool HasBehaviour(AbilityBehavior behavior)
        => AbilityBehavior.HasFlag(behavior);

    public List<AbilityBehavior> GetBehaviors()
    {
        var behaviors = new List<AbilityBehavior>();
        foreach (AbilityBehavior b in Enum.GetValues(typeof(AbilityBehavior)))
        {
            if (HasBehaviour(b)) behaviors.Add(b);
        }
        return behaviors;
    }

    public List<AbilityBehavior> GetTargetType()
    {
        var targetTypes = new List<AbilityBehavior>();
        if (HasBehaviour(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_PASSIVE)) targetTypes.Add(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_PASSIVE);
        if (HasBehaviour(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_NO_TARGET)) targetTypes.Add(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_NO_TARGET);
        if (HasBehaviour(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_UNIT_TARGET)) targetTypes.Add(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_UNIT_TARGET);
        if (HasBehaviour(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_POINT)) targetTypes.Add(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_POINT);
        return targetTypes;
    }

    public List<string> GetTargetTypeNames()
    {
        var targetNames = new List<string>();
        if (HasBehaviour(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_PASSIVE)) targetNames.Add("Passive");
        if (HasBehaviour(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_NO_TARGET)) targetNames.Add("No Target");
        if (HasBehaviour(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_UNIT_TARGET)) targetNames.Add("Unit Target");
        if (HasBehaviour(AbilityBehavior.DOTA_ABILITY_BEHAVIOR_POINT)) targetNames.Add("Point Target");
        return targetNames;
    }

    public List<AbilityValue> GetItemBonusValues()
    {
        var bonusValues = new List<AbilityValue>();

        foreach (var value in AbilityValues)
        {
            if (!string.IsNullOrEmpty(value.Description) && (value.Description.StartsWith('+') || value.Description.StartsWith('-')))
            {
                bonusValues.Add(value);
            }
        }
        return bonusValues;
    }
}
