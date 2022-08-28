using System.ComponentModel.DataAnnotations;
namespace Magus.Data.Models.Dota
{
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
        public IEnumerable<string> Notes { get; set; }

        public string LocalShard { get; set; } // needed?
        public string LocalScepter { get; set; } // needed?

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
        public IEnumerable<float>? AbilityCastRange { get; set; }
        public IEnumerable<float>? AbilityCastPoint { get; set; }
        public IEnumerable<float>? AbilityChannelTime { get; set; }
        public IEnumerable<float>? AbilityCooldown { get; set; }
        public IEnumerable<float>? AbilityDuration { get; set; }
        public IEnumerable<float>? AbilityDamage { get; set; }
        public IEnumerable<float>? AbilityManaCost { get; set; }

        public IEnumerable<AbilityValue> AbilityValues { get; set; }

        public record AbilityValue
        {
            public string Name { get; set; }
            public string? Description { get; set; }
            public IEnumerable<float> Values { get; set; }
            public string? LinkedSpecialBonus { get; set; }
            public string? SpecialBonusValue { get; set; }
            public bool RequiresScepter { get; set; }
            public bool RequiresShard { get; set; }
        }

        // Is there anything we don't know?
        public Dictionary<string, object>? ExtensionData { get; set; }

        public bool HasBehaviour(AbilityBehavior behavior)
            => ((AbilityBehavior)AbilityBehavior).HasFlag(behavior);

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

        public List<AbilityValue> GetSpellValues()
        {
            var spellValues = new List<AbilityValue>();

            foreach (var value in AbilityValues)
            {
                if (!string.IsNullOrEmpty(value.Description) && !(value.Description.StartsWith('+') || value.Description.StartsWith('-')))
                {
                    spellValues.Add(value);
                }
            }

            return spellValues;
        }
    }

    [Flags]
    public enum AbilityBehavior : ulong
    {
        DOTA_ABILITY_BEHAVIOR_NONE                           = 0,
        DOTA_ABILITY_BEHAVIOR_HIDDEN                         = 1,
        [Display(Name = "Passive")]
        DOTA_ABILITY_BEHAVIOR_PASSIVE                        = 2,
        [Display(Name = "No Target")]
        DOTA_ABILITY_BEHAVIOR_NO_TARGET                      = 4,
        [Display(Name = "Unit Target")]
        DOTA_ABILITY_BEHAVIOR_UNIT_TARGET                    = 8,
        [Display(Name = "Point target")]
        DOTA_ABILITY_BEHAVIOR_POINT                          = 16,
        DOTA_ABILITY_BEHAVIOR_AOE                            = 32,
        DOTA_ABILITY_BEHAVIOR_NOT_LEARNABLE                  = 64,
        DOTA_ABILITY_BEHAVIOR_CHANNELLED                     = 128,
        DOTA_ABILITY_BEHAVIOR_ITEM                           = 256,
        DOTA_ABILITY_BEHAVIOR_TOGGLE                         = 512,
        DOTA_ABILITY_BEHAVIOR_DIRECTIONAL                    = 1024,
        DOTA_ABILITY_BEHAVIOR_IMMEDIATE                      = 2048,
        DOTA_ABILITY_BEHAVIOR_AUTOCAST                       = 4096,
        DOTA_ABILITY_BEHAVIOR_OPTIONAL_UNIT_TARGET           = 8192,
        DOTA_ABILITY_BEHAVIOR_OPTIONAL_POINT                 = 16384,
        DOTA_ABILITY_BEHAVIOR_OPTIONAL_NO_TARGET             = 32768,
        DOTA_ABILITY_BEHAVIOR_AURA                           = 65536,
        DOTA_ABILITY_BEHAVIOR_ATTACK                         = 131072,
        DOTA_ABILITY_BEHAVIOR_DONT_RESUME_MOVEMENT           = 262144,
        DOTA_ABILITY_BEHAVIOR_ROOT_DISABLES                  = 524288,
        DOTA_ABILITY_BEHAVIOR_UNRESTRICTED                   = 1048576,
        DOTA_ABILITY_BEHAVIOR_IGNORE_PSEUDO_QUEUE            = 2097152,
        DOTA_ABILITY_BEHAVIOR_IGNORE_CHANNEL                 = 4194304,
        DOTA_ABILITY_BEHAVIOR_DONT_CANCEL_MOVEMENT           = 8388608,
        DOTA_ABILITY_BEHAVIOR_DONT_ALERT_TARGET              = 16777216,
        DOTA_ABILITY_BEHAVIOR_DONT_RESUME_ATTACK             = 33554432,
        DOTA_ABILITY_BEHAVIOR_NORMAL_WHEN_STOLEN             = 67108864,
        DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING               = 134217728,
        DOTA_ABILITY_BEHAVIOR_RUNE_TARGET                    = 268435456,
        DOTA_ABILITY_BEHAVIOR_DONT_CANCEL_CHANNEL            = 536870912,
        DOTA_ABILITY_BEHAVIOR_VECTOR_TARGETING               = 1073741824,
        DOTA_ABILITY_BEHAVIOR_LAST_RESORT_POINT              = 2147483648,
        DOTA_ABILITY_BEHAVIOR_CAN_SELF_CAST                  = 4294967296,
        DOTA_ABILITY_BEHAVIOR_SHOW_IN_GUIDES                 = 8589934592,
        DOTA_ABILITY_BEHAVIOR_UNLOCKED_BY_EFFECT_INDEX       = 17179869184,
        DOTA_ABILITY_BEHAVIOR_SUPPRESS_ASSOCIATED_CONSUMABLE = 34359738368,
        DOTA_ABILITY_BEHAVIOR_FREE_DRAW_TARGETING            = 68719476736,
    }

    [Flags]
    public enum AbilityUnitDamageType : byte
    {
        DAMAGE_TYPE_NONE       = 0,
        [Display(Name = "Physical")]
        DAMAGE_TYPE_PHYSICAL   = 1,
        [Display(Name = "Magical")]
        DAMAGE_TYPE_MAGICAL    = 2,
        [Display(Name = "Pure")]
        DAMAGE_TYPE_PURE       = 4,
        [Display(Name = "HP Removal")]
        DAMAGE_TYPE_HP_REMOVAL = 8,
        DAMAGE_TYPE_ALL        = 7,
    }
    public enum AbilityType : byte
    {
        DOTA_ABILITY_TYPE_BASIC      = 0,
        DOTA_ABILITY_TYPE_ULTIMATE   = 1,
        DOTA_ABILITY_TYPE_ATTRIBUTES = 2,
        HIDDEN                       = 3, // needed?
    }

    public enum SpellImmunityType : byte
    {
        SPELL_IMMUNITY_NONE                  = 0,
        SPELL_IMMUNITY_ALLIES_YES            = 1,
        SPELL_IMMUNITY_ALLIES_NO             = 2,
        SPELL_IMMUNITY_ENEMIES_YES           = 3,
        SPELL_IMMUNITY_ENEMIES_NO            = 4,
        SPELL_IMMUNITY_ALLIES_YES_ENEMIES_NO = 5,
    }

    public enum SpellDispellableType : byte
    {
        SPELL_DISPELLABLE_NONE       = 0,
        [Display(Name = "Strong Dispels")]
        SPELL_DISPELLABLE_YES_STRONG = 1,
        [Display(Name = "Basic Dispel")]
        SPELL_DISPELLABLE_YES        = 2,
        [Display(Name = "No")]
        SPELL_DISPELLABLE_NO         = 3,
    }

    [Flags]
    public enum DamageFlag
    {
        NONE                                 = 0,
        IGNORES_MAGIC_ARMOR                  = 1,
        IGNORES_PHYSICAL_ARMOR               = 2,
        BYPASSES_INVULNERABILITY             = 4,
        BYPASSES_BLOCK                       = 8,
        REFLECTION                           = 16,
        HPLOSS                               = 32,
        NO_DIRECTOR_EVENT                    = 64,
        NON_LETHAL                           = 128,
        USE_COMBAT_PROFICIENCY               = 256,
        NO_DAMAGE_MULTIPLIERS                = 512,
        NO_SPELL_AMPLIFICATION               = 1024,
        DONT_DISPLAY_DAMAGE_IF_SOURCE_HIDDEN = 2048,
        NO_SPELL_LIFESTEAL                   = 4096,
        PROPERTY_FIRE                        = 8192,
        IGNORES_BASE_PHYSICAL_ARMOR          = 16384,
    }

    [Flags]
    public enum AbilityUnitTargetTeam : byte
    {
        DOTA_UNIT_TARGET_TEAM_NONE     = 0,
        [Display(Name = "Allies")]
        DOTA_UNIT_TARGET_TEAM_FRIENDLY = 1,
        [Display(Name = "Enemy")]
        DOTA_UNIT_TARGET_TEAM_ENEMY    = 2,
        [Display(Name = "Both")]
        DOTA_UNIT_TARGET_TEAM_BOTH     = 3,
        DOTA_UNIT_TARGET_TEAM_CUSTOM   = 4,
        FORCEABILITY                   = 7, // Friendly, enemy, and custom
    }

    [Flags]
    public enum AbilityUnitTargetType : byte
    {
        DOTA_UNIT_TARGET_NONE     = 0,
        DOTA_UNIT_TARGET_HERO     = 1,
        DOTA_UNIT_TARGET_CREEP    = 2,
        DOTA_UNIT_TARGET_BUILDING = 4,
        DOTA_UNIT_TARGET_COURIER  = 16,
        DOTA_UNIT_TARGET_BASIC    = 18,
        DOTA_UNIT_TARGET_OTHER    = 32,
        DOTA_UNIT_TARGET_ALL      = 55,
        DOTA_UNIT_TARGET_TREE     = 64,
        DOTA_UNIT_TARGET_CUSTOM   = 128,
    }

    [Flags]
    public enum UnitTargetFlags
    {
        NONE                    = 0,
        RANGED_ONLY             = 2,
        MELEE_ONLY              = 4,
        DEAD                    = 8,
        MAGIC_IMMUNE_ENEMIES    = 16,
        NOT_MAGIC_IMMUNE_ALLIES = 32,
        INVULNERABLE            = 64,
        FOW_VISIBLE             = 128,
        NO_INVIS                = 256,
        NOT_ANCIENTS            = 512,
        PLAYER_CONTROLLED       = 1024,
        NOT_DOMINATED           = 2048,
        NOT_SUMMONED            = 4096,
        NOT_ILLUSIONS           = 8192,
        NOT_ATTACK_IMMUNE       = 16384,
        MANA_ONLY               = 32768,
        CHECK_DISABLE_HELP      = 65536,
        NOT_CREEP_HERO          = 131072,
        OUT_OF_WORLD            = 262144,
        NOT_NIGHTMARED          = 524288,
        PREFER_ENEMIES          = 1048576,
        RESPECT_OBSTRUCTIONS    = 2097152,
    }
}
