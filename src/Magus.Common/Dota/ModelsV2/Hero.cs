using Magus.Common.Dota.Enums;
using System.Diagnostics;

namespace Magus.Common.Dota.ModelsV2;

[DebuggerDisplay("{InternalName}")]
public class Hero
{
    // Info
    public required string InternalName { get; init; }

    public int Id { get; init; }

    public short HeroOrderId { get; init; }

    public required string[] NameAliases { get; init; } // Not localised

    public uint HeroGlowColor { get; init; } // uint Bitmask for now. Not all heroes have it.

    public required int[] SimilarHeroes { get; init; }

    /// <summary>
    /// Key: Ability index, 1 base
    /// <br/>
    /// Value: Ability Name
    /// </summary>
    /// <remarks>
    /// Dictionary count may be less than max index, sometimes indexes are skipped. 
    /// </remarks>
    public required Dictionary<int, string> Abilities { get; init; }

    public int AbilityTalentStart { get; init; }

    public required Facet[] Facets { get; init; }

    // Attributes
    public AttributePrimary AttributePrimary { get; init; }

    public short AttributeBaseAgility { get; init; }

    public float AttributeAgilityGain { get; init; }

    public short AttributeBaseStrength { get; init; }

    public float AttributeStrengthGain { get; init; }

    public short AttributeBaseIntelligence { get; init; }

    public float AttributeIntelligenceGain { get; init; }

    // Role
    public byte Complexity { get; init; }

    public required Role[] Role { get; init; }

    public required byte[] Rolelevels { get; init; }

    // Stats
    public AttackCapabilities AttackCapabilities { get; init; }

    public short AttackDamageMin { get; init; }

    public short AttackDamageMax { get; init; }

    public float AttackRate { get; init; }

    public short BaseAttackSpeed { get; init; }

    public float AttackAnimationPoint { get; init; }

    public float AttackRange { get; init; }

    public float ProjectileSpeed { get; init; }

    public short ArmorPhysical { get; init; }

    public short MagicalResistance { get; init; }

    public short MovementSpeed { get; init; }

    public float MovementTurnRate { get; init; }

    public short VisionDaytimeRange { get; init; }

    public short VisionNighttimeRange { get; init; }

    public short StatusHealth { get; init; }

    public float StatusHealthRegen { get; init; }

    public short StatusMana { get; init; }

    public float StatusManaRegen { get; init; }

    public int GetRoleLevel(Role role)
        => Rolelevels[Array.IndexOf(Role, role)];

    public Role[] GetHightestRoles()
        => Rolelevels.Select((value, index) => (value, index))
            .Where(x => x.value == Rolelevels.Max())
            .Select(x => Role[x.index])
            .ToArray();

    public string GetAttackType()
        => AttackCapabilities.ToString(); // TODO localise/remove

    public double GetAttackDamageMin()
        => AttributePrimary switch
        {
            AttributePrimary.DOTA_ATTRIBUTE_AGILITY   => AttackDamageMin + AttributeBaseAgility,
            AttributePrimary.DOTA_ATTRIBUTE_INTELLECT => AttackDamageMin + AttributeBaseIntelligence,
            AttributePrimary.DOTA_ATTRIBUTE_STRENGTH  => AttackDamageMin + AttributeBaseStrength,
            AttributePrimary.DOTA_ATTRIBUTE_ALL       => AttackDamageMin + (AttributeBaseTotal * 0.7),

            _ => throw new ArgumentOutOfRangeException(nameof(AttributePrimary), AttributePrimary, null)
        };

    public double GetAttackDamageMax()
        => AttributePrimary switch
        {
            AttributePrimary.DOTA_ATTRIBUTE_AGILITY   => AttackDamageMax + AttributeBaseAgility,
            AttributePrimary.DOTA_ATTRIBUTE_INTELLECT => AttackDamageMax + AttributeBaseIntelligence,
            AttributePrimary.DOTA_ATTRIBUTE_STRENGTH  => AttackDamageMax + AttributeBaseStrength,
            AttributePrimary.DOTA_ATTRIBUTE_ALL       => AttackDamageMax + (AttributeBaseTotal * 0.7),

            _ => throw new ArgumentOutOfRangeException(nameof(AttributePrimary), AttributePrimary, null)
        };

    public int AttributeBaseTotal
        => AttributeBaseStrength + AttributeBaseAgility + AttributeBaseIntelligence;

    public float GetAttackTime()
        => 1 / ((BaseAttackSpeed + AttributeBaseAgility) / (100 * AttackRate));

    public float GetArmor()
        => ArmorPhysical + (AttributeBaseAgility / 6f);
}
