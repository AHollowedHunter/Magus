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
        public IEnumerable<string>? NameAliases { get; set; }
        public string? RealName { get; set; } // Manually configure these
        public string Bio { get; set; } // lore
        public string Hype { get; set; }
        public string NpeDesc { get; set; }
        public short OrderId { get; set; }

        // Spells        
        public Ability[] Abilities { get; set; }
        public Talent[] Talents { get; set; }

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
        public Role[] Roles { get; set; }
        public int[] RoleLevels { get; set; }

        // Stats
        public AttackCapabilities AttackCapabilities { get; set; }
        public byte DamageMin { get; set; }
        public byte DamageMax { get; set; }
        public float AttackRate { get; set; }
        public short AttackRange { get; set; }
        public short ProjectileSpeed { get; set; }
        public float Armour { get; set; }
        public byte MagicResistance { get; set; } = 25; // ALl heroes have base 25%
        public short MoveSpeed { get; set; }
        public float TurnRate { get; set; }
        public short ViewRangeDay { get; set; }
        public short ViewRangeNight { get; set; }
        public short HealthMax { get; set; }
        public float HealthRegen { get; set; }
        public short ManaMax { get; set; }
        public float ManaRegen { get; set; }


        // Is there anything we don't know?
        public Dictionary<string, object>? ExtensionData { get; set; }

        public int GetRoleLevel(Role role)
            => RoleLevels[(int)role];

        public Role[] GetHightestRoles()
            => RoleLevels.Select((value, index) => new { value, index }).Where(x => x.value == RoleLevels.Max()).Select(x => (Role)x.index).ToArray();

        public Role[] GetAllRoles()
            => RoleLevels.Select((value, index) => new { value, index }).Where(x => x.value != 0).Select(x => (Role)x.index).ToArray();

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
        setiator
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
