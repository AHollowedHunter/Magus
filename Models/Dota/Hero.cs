using System.ComponentModel.DataAnnotations;

namespace Magus.Data.Models.Dota
{
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
        public IEnumerable<Ability> Abilities { get; set; }
        public IEnumerable<Talent> Talents { get; set; }

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
        public float AttackRate { get; set; }      = 1.7F;
        public short BaseAttackSpeed { get; set; } = 100; 
        public float AttackAnimationPoint { get; set; }
        public float AttackRange { get; set; }
        public float ProjectileSpeed { get; set; }
        public short ArmorPhysical { get; set; }
        public short MagicalResistance { get; set; } = 25; // ALl heroes have base 25%
        public short MovementSpeed { get; set; }
        public float MovementTurnRate { get; set; }     = 0.6F; // Default. Maybe create the base hero and infer defaults from that in future if valve changes these
        public short VisionDaytimeRange { get; set; }   = 1800;
        public short VisionNighttimeRange { get; set; } = 800;
        public short StatusHealth { get; set; }         = 200; // Default is 200, rest comes from strength
        public float StatusHealthRegen { get; set; }    = 0.25F; // This is bonus regen, ignoring Strength based HP regen
        public short StatusMana { get; set; }           = 75; // Default is 75, rest from intellect
        public float StatusManaRegen { get; set; }      = 0; //Bonus mana regen, ignoring Intellect base regen


        // Is there anything we don't know?
        public Dictionary<string, object>? ExtensionData { get; set; }

        public int GetRoleLevel(Role role)
            => Rolelevels[(int)role];

        public Role[] GetHightestRoles()
            => Rolelevels.Select((value, index) => new { value, index }).Where(x => x.value == Rolelevels.Max()).Select(x => (Role)x.index).ToArray();

        public Role[] GetAllRoles()
            => Rolelevels.Select((value, index) => new { value, index }).Where(x => x.value != 0).Select(x => (Role)x.index).ToArray();

        public string GetAttackType()
            => AttackCapabilities.ToString();
    }

    public enum AttributePrimary
    {
        [Display(Name = "Strength")]
        DOTA_ATTRIBUTE_STRENGTH,
        [Display(Name = "Agility")]
        DOTA_ATTRIBUTE_AGILITY,
        [Display(Name = "Intelligence")]
        DOTA_ATTRIBUTE_INTELLECT
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
}
