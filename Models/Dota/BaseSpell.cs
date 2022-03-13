using System.Text.Json;
using System.Text.Json.Serialization;

namespace Magus.Data.Models.Dota
{
    /// <summary>
    /// DotA appears to handle hero abilities & talents along with items as the same identical "ability".
    /// Have them all inherit from a base class, which I decided to call "spells"
    /// </summary>
    public abstract record BaseSpell
    {
        // Info
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name_loc")]
        public string LocalName { get; set; }
        [JsonPropertyName("name")]
        public string InternalName { get; set; }
        [JsonPropertyName("desc_loc")]
        public string LocalDesc { get; set; }
        [JsonPropertyName("lore_loc")]
        public string LocalLore { get; set; }
        [JsonPropertyName("notes_loc")]
        public string[] LocalNotes { get; set; }
        [JsonPropertyName("shard_loc")]
        public string LocalShard { get; set; }
        [JsonPropertyName("scepter_loc")]
        public string LocalScepter { get; set; }
        [JsonPropertyName("type")]
        public byte Type { get; set; }
        [JsonPropertyName("behavior")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long Behaviour { get; set; }
        [JsonPropertyName("target_team")]
        public UnitTargetTeam TargetTeam { get; set; }
        [JsonPropertyName("target_type")]
        public UnitTargetType TargetType { get; set; }
        [JsonPropertyName("flags")]
        public int Flags { get; set; }
        [JsonPropertyName("damage")]
        public DamageType Damage { get; set; }
        [JsonPropertyName("immunity")]
        public SpellImmunityType Immunity { get; set; }
        [JsonPropertyName("dispellable")]
        public Dispellable Dispellable { get; set; }
        [JsonPropertyName("max_level")]
        public byte LevelMax { get; set; }

        // Stats
        [JsonPropertyName("cast_ranges")]
        public int[] CastRanges { get; set; }
        [JsonPropertyName("cast_points")]
        public float[] CastPoints { get; set; }
        [JsonPropertyName("channel_times")]
        public float[] ChannelTimes { get; set; }
        [JsonPropertyName("cooldowns")]
        public float[] Cooldowns { get; set; }
        [JsonPropertyName("durations")]
        public float[] Durations { get; set; }
        [JsonPropertyName("damages")]
        public float[] Damages { get; set; }
        [JsonPropertyName("mana_costs")]
        public short[] ManaCosts { get; set; }
        [JsonPropertyName("gold_costs")]
        public short[] GoldCosts { get; set; }

        [JsonPropertyName("special_values")]
        public SpecialValues[] SpecialValues { get; set; }

        // Item Properties
        [JsonPropertyName("is_item")]
        public bool IsItem { get; set; }
        [JsonPropertyName("item_cost")]
        public short ItemCost { get; set; }
        [JsonPropertyName("item_initial_charges")]
        public byte ItemInitialCharges { get; set; }
        [JsonPropertyName("item_neutral_tier")]
        public uint ItemNeutralTier { get; set; }
        [JsonPropertyName("item_stock_max")]
        public byte ItemStockMax { get; set; }
        [JsonPropertyName("item_stock_time")]
        public int ItemStockTime { get; set; }
        [JsonPropertyName("item_quality")]
        public byte ItemQuality { get; set; }


        // Ability properties
        [JsonPropertyName("ability_has_scepter")]
        public bool AbilityHasScepter { get; set; }
        [JsonPropertyName("ability_has_shard")]
        public bool AbilityHasShard { get; set; }
        [JsonPropertyName("ability_is_granted_by_scepter")]
        public bool AbilityIsGrantedByScepter { get; set; }
        [JsonPropertyName("ability_is_granted_by_shard")]
        public bool AbilityIsGrantedByShard { get; set; }

        // Is there anything we don't know?
        [JsonExtensionData]
        public Dictionary<string, object>? ExtensionData { get; set; }

        public bool HasBehaviour(BehaviorFlags behavior)
            => ((BehaviorFlags)Behaviour).HasFlag(behavior);

        public List<BehaviorFlags> GetBehaviors()
        {
            var behaviors = new List<BehaviorFlags>();
            foreach (BehaviorFlags b in Enum.GetValues(typeof(BehaviorFlags)))
            {
                if (HasBehaviour(b)) behaviors.Add(b);
            }
            return behaviors;
        }

        public List<BehaviorFlags> GetTargetType()
        {
            var targetTypes = new List<BehaviorFlags>();
            if (HasBehaviour(BehaviorFlags.PASSIVE)) targetTypes.Add(BehaviorFlags.PASSIVE);
            if (HasBehaviour(BehaviorFlags.NO_TARGET)) targetTypes.Add(BehaviorFlags.NO_TARGET);
            if (HasBehaviour(BehaviorFlags.UNIT_TARGET)) targetTypes.Add(BehaviorFlags.UNIT_TARGET);
            if (HasBehaviour(BehaviorFlags.POINT)) targetTypes.Add(BehaviorFlags.POINT);
            return targetTypes;
        }

        public List<string> GetTargetTypeNames()
        {
            var targetNames = new List<string>();
            if (HasBehaviour(BehaviorFlags.PASSIVE)) targetNames.Add("Passive");
            if (HasBehaviour(BehaviorFlags.NO_TARGET)) targetNames.Add("No Target");
            if (HasBehaviour(BehaviorFlags.UNIT_TARGET)) targetNames.Add("Unit Target");
            if (HasBehaviour(BehaviorFlags.POINT)) targetNames.Add("Point Target");
            return targetNames;
        }

        public UnitTargetTeam GetTargetTeam()
            => (UnitTargetTeam)TargetTeam;

        public UnitTargetType GetUnitTargetType()
            => (UnitTargetType)TargetType;

        public DamageType GetDamageType()
            => (DamageType)Damage;

        public AbilityType GetAbilityType()
            => (AbilityType)Type;

        public SpellImmunityType GetSpellImmunityType()
            => (SpellImmunityType)Immunity;

        public Dispellable GetDispellable()
            => (Dispellable)Dispellable;

        public List<SpecialValues> GetItemBonusValues()
        {
            var bonusValues = new List<SpecialValues>();

            foreach (var value in SpecialValues)
            {
                if (!string.IsNullOrEmpty(value.LocalHeading) && (value.LocalHeading.StartsWith('+') || value.LocalHeading.StartsWith('-')))
                {
                    bonusValues.Add(value);
                }
            }
            return bonusValues;
        }

        public List<SpecialValues> GetSpellValues()
        {
            var spellValues = new List<SpecialValues>();

            foreach (var value in SpecialValues)
            {
                if (!string.IsNullOrEmpty(value.LocalHeading) && !(value.LocalHeading.StartsWith('+') || value.LocalHeading.StartsWith('-')))
                {
                    spellValues.Add(value);
                }
            }

            return spellValues;
        }
    }

    public record SpecialValues
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("values_float")]
        public float[] ValuesFloat { get; set; }
        [JsonPropertyName("values_int")]
        public int[] ValuesInt { get; set; }
        [JsonPropertyName("is_percentage")]
        public bool IsPercentage { get; set; }
        [JsonPropertyName("heading_loc")]
        public string LocalHeading { get; set; }
    }

    [Flags]
    public enum BehaviorFlags : long
    {
        HIDDEN = 1,
        PASSIVE = 2,
        NO_TARGET = 4,
        UNIT_TARGET = 8,
        POINT = 16,
        AOE = 32,
        NOT_LEARNABLE = 64,
        CHANNELLED = 128,
        ITEM = 256,
        TOGGLE = 512,
        DIRECTIONAL = 1024,
        IMMEDIATE = 2048,
        AUTOCAST = 4096,
        OPTIONAL_UNIT_TARGET = 8192,
        OPTIONAL_POINT = 16384,
        OPTIONAL_NO_TARGET = 32768,
        AURA = 65536,
        ATTACK = 131072,
        DONT_RESUME_MOVEMENT = 262144,
        ROOT_DISABLES = 524288,
        UNRESTRICTED = 1048576,
        IGNORE_PSEUDO_QUEUE = 2097152,
        IGNORE_CHANNEL = 4194304,
        DONT_CANCEL_MOVEMENT = 8388608,
        DONT_ALERT_TARGET = 16777216,
        DONT_RESUME_ATTACK = 33554432,
        NORMAL_WHEN_STOLEN = 67108864,
        IGNORE_BACKSWING = 134217728,
        RUNE_TARGET = 268435456,
        DONT_CANCEL_CHANNEL = 536870912,
        VECTOR_TARGETING = 1073741824,
        LAST_RESORT_POINT = 2147483648,
        CAN_SELF_CAST = 4294967296,
        SHOW_IN_GUIDES = 8589934592,
        UNLOCKED_BY_EFFECT_INDEX = 17179869184,
        SUPPRESS_ASSOCIATED_CONSUMABLE = 34359738368,
        FREE_DRAW_TARGETING = 68719476736,
    }

    [Flags]
    public enum DamageType
    {
        NONE = 0,
        PHYSICAL = 1,
        MAGICAL = 2,
        PURE = 4,
        HP_REMOVAL = 8,
        ALL = 7,
    }
    public enum AbilityType
    {
        BASIC = 0,
        ULTIMATE = 1,
        ATTRIBUTES = 2,
        HIDDEN = 3,
    }

    public enum SpellImmunityType
    {
        NONE = 0,
        ALLIES_YES = 1,
        ALLIES_NO = 2,
        ENEMIES_YES = 3,
        ENEMIES_NO = 4,
        ALLIES_YES_ENEMIES_NO = 5,
    }

    public enum Dispellable
    {
        NONE = 0,
        STRONG = 1,
        BASIC = 2,
        NO = 3,
    }

    [Flags]
    public enum DamageFlag
    {
        NONE = 0,
        IGNORES_MAGIC_ARMOR = 1,
        IGNORES_PHYSICAL_ARMOR = 2,
        BYPASSES_INVULNERABILITY = 4,
        BYPASSES_BLOCK = 8,
        REFLECTION = 16,
        HPLOSS = 32,
        NO_DIRECTOR_EVENT = 64,
        NON_LETHAL = 128,
        USE_COMBAT_PROFICIENCY = 256,
        NO_DAMAGE_MULTIPLIERS = 512,
        NO_SPELL_AMPLIFICATION = 1024,
        DONT_DISPLAY_DAMAGE_IF_SOURCE_HIDDEN = 2048,
        NO_SPELL_LIFESTEAL = 4096,
        PROPERTY_FIRE = 8192,
        IGNORES_BASE_PHYSICAL_ARMOR = 16384,
    }

    [Flags]
    public enum UnitTargetTeam
    {
        NONE = 0,
        FRIENDLY = 1,
        ENEMY = 2,
        BOTH = 3,
        CUSTOM = 4,
        FORCEABILITY = 7, //wtf valve  "Force Staff" "Hurricane Pike" "Force Boots"
    }

    public enum UnitTargetType
    {
        NONE = 0,
        HERO = 1,
        CREEP = 2,
        BUILDING = 4,
        COURIER = 16,
        OTHER = 32,
        TREE = 64,
        CUSTOM = 128,
        BASIC = 18,
        ALL = 55,
    }

    [Flags]
    public enum UnitTargetFlags
    {
        NONE = 0,
        RANGED_ONLY = 2,
        MELEE_ONLY = 4,
        DEAD = 8,
        MAGIC_IMMUNE_ENEMIES = 16,
        NOT_MAGIC_IMMUNE_ALLIES = 32,
        INVULNERABLE = 64,
        FOW_VISIBLE = 128,
        NO_INVIS = 256,
        NOT_ANCIENTS = 512,
        PLAYER_CONTROLLED = 1024,
        NOT_DOMINATED = 2048,
        NOT_SUMMONED = 4096,
        NOT_ILLUSIONS = 8192,
        NOT_ATTACK_IMMUNE = 16384,
        MANA_ONLY = 32768,
        CHECK_DISABLE_HELP = 65536,
        NOT_CREEP_HERO = 131072,
        OUT_OF_WORLD = 262144,
        NOT_NIGHTMARED = 524288,
        PREFER_ENEMIES = 1048576,
        RESPECT_OBSTRUCTIONS = 2097152,
    }
}
