using System.ComponentModel.DataAnnotations;

namespace Magus.Data.Models.Dota;

public record Hero
{
    // Info        
    public int Id { get; set; }
    public string Language { get; set; }
    public string InternalName { get; set; }
    public string Name { get; set; }
    public IEnumerable<string> NameAliases { get; set; }
    public string? RealName { get; set; } // Manually configure these
    public string Bio { get; set; } // lore
    public string Hype { get; set; }
    public string NpeDesc { get; set; }
    public short HeroOrderID { get; set; }

    // Spells        
    public IList<Ability> Abilities { get; set; }
    public IList<Talent> Talents { get; set; } // First is 10 right, 2nd left, 3rd 15 right, etc...

    // Attributes
    public byte AttributeBaseAgility { get; set; }
    public float AttributeAgilityGain { get; set; }
    public byte AttributeBaseStrength { get; set; }
    public float AttributeStrengthGain { get; set; }
    public byte AttributeBaseIntelligence { get; set; }
    public float AttributeIntelligenceGain { get; set; }
    public AttributePrimary AttributePrimary { get; set; }

    // Role        
    public byte Complexity { get; set; }
    public Role[] Role { get; set; }
    public byte[] Rolelevels { get; set; }

    // Stats
    public AttackCapabilities AttackCapabilities { get; set; }
    public short AttackDamageMin { get; set; }
    public short AttackDamageMax { get; set; }
    public float AttackRate { get; set; }
    public short BaseAttackSpeed { get; set; }
    public float AttackAnimationPoint { get; set; }
    public float AttackRange { get; set; }
    public float ProjectileSpeed { get; set; }
    public short ArmorPhysical { get; set; }
    public short MagicalResistance { get; set; }
    public short MovementSpeed { get; set; }
    public float MovementTurnRate { get; set; }
    public short VisionDaytimeRange { get; set; }
    public short VisionNighttimeRange { get; set; }
    public short StatusHealth { get; set; }
    public float StatusHealthRegen { get; set; }
    public short StatusMana { get; set; }
    public float StatusManaRegen { get; set; }

    public int GetRoleLevel(Role role)
        => Rolelevels[Array.IndexOf(Role, role)];

    public Role[] GetHightestRoles()
        => Rolelevels.Select((value, index) => new { value, index })
                     .Where(x => x.value == Rolelevels.Max())
                     .Select(x => Role[x.index])
                     .ToArray();

    public string GetAttackType()
        => AttackCapabilities.ToString();

    public double GetAttackDamageMin()
        => AttributePrimary switch
        {
            AttributePrimary.DOTA_ATTRIBUTE_AGILITY   => AttackDamageMin + AttributeBaseAgility,
            AttributePrimary.DOTA_ATTRIBUTE_INTELLECT => AttackDamageMin + AttributeBaseIntelligence,
            AttributePrimary.DOTA_ATTRIBUTE_STRENGTH  => AttackDamageMin + AttributeBaseStrength,
            AttributePrimary.DOTA_ATTRIBUTE_ALL       => AttackDamageMin + (AttributeBaseTotal * 0.7),
            _                                         => AttackDamageMin,
        };

    public double GetAttackDamageMax()
        => AttributePrimary switch
        {
            AttributePrimary.DOTA_ATTRIBUTE_AGILITY   => AttackDamageMax + AttributeBaseAgility,
            AttributePrimary.DOTA_ATTRIBUTE_INTELLECT => AttackDamageMax + AttributeBaseIntelligence,
            AttributePrimary.DOTA_ATTRIBUTE_STRENGTH  => AttackDamageMax + AttributeBaseStrength,
            AttributePrimary.DOTA_ATTRIBUTE_ALL       => AttackDamageMax + (AttributeBaseTotal * 0.7),
            _                                         => AttackDamageMax
        };

    public int AttributeBaseTotal
        => AttributeBaseStrength + AttributeBaseAgility + AttributeBaseIntelligence;

    public float GetAttackTime()
        => 1 / ((BaseAttackSpeed + AttributeBaseAgility) / (100 * AttackRate));

    public float GetArmor()
        => ArmorPhysical + (AttributeBaseAgility / 6);
}

public enum AttributePrimary
{
    [Display(Name = "Strength")]
    DOTA_ATTRIBUTE_STRENGTH,
    [Display(Name = "Agility")]
    DOTA_ATTRIBUTE_AGILITY,
    [Display(Name = "Intelligence")]
    DOTA_ATTRIBUTE_INTELLECT,
    [Display(Name = "Universal")]
    DOTA_ATTRIBUTE_ALL
}

public enum Role
{
    Carry,
    Support,
    Nuker,
    Disabler,
    Jungler,
    Durable,
    Escape,
    Pusher,
    Initiator
}

public enum AttackCapabilities
{
    [Display(Name = "No Attack")]
    DOTA_UNIT_CAP_NO_ATTACK                 = 0,
    [Display(Name = "Melee")]
    DOTA_UNIT_CAP_MELEE_ATTACK              = 1,
    [Display(Name = "Ranged")]
    DOTA_UNIT_CAP_RANGED_ATTACK             = 2,
    [Display(Name = "Ranged, Direction")]
    DOTA_UNIT_CAP_RANGED_ATTACK_DIRECTIONAL = 4,
}
