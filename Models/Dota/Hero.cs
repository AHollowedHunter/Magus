using System.Text.Json;
using System.Text.Json.Serialization;

namespace Magus.Data.Models.Dota
{
    public record Hero
    {
        public Hero()
        {

        }

        // Info
        [JsonPropertyName("id")]
        public int Id { get; init; }
        [JsonPropertyName("name_loc")]
        public string LocalName { get; init; }
        [JsonPropertyName("name")]
        public string InternalName { get; init; }
        [JsonPropertyName("real_name")]
        public string? RealName { get; init; }
        [JsonPropertyName("bio_loc")]
        public string LocalBio { get; init; }
        [JsonPropertyName("hype_loc")]
        public string LocalHype { get; init; }
        [JsonPropertyName("npe_desc_loc")]
        public string LocalNpeDesc { get; init; }
        [JsonPropertyName("order_id")]
        public short OrderId { get; init; }

        // Spells
        [JsonPropertyName("abilities")]
        public Ability[] Abilities { get; init; }
        [JsonPropertyName("talents")]
        public Talent[] Talents { get; init; }


        // Attributes
        [JsonPropertyName("str_base")]
        public byte StrengthBase { get; init; }
        [JsonPropertyName("str_gain")]
        public float StrengthGain { get; init; }
        [JsonPropertyName("agi_base")]
        public byte AgilityBase { get; init; }
        [JsonPropertyName("agi_gain")]
        public float AgilityGain { get; init; }
        [JsonPropertyName("int_base")]
        public byte IntelligenceBase { get; init; }
        [JsonPropertyName("int_gain")]
        public float IntelligenceGain { get; init; }
        [JsonPropertyName("primary_attr")]
        public PrimaryAttribute PrimaryAttribute { get; init; }

        // Role
        [JsonPropertyName("complexity")]
        public byte Complexity { get; init; }
        [JsonPropertyName("attack_capability")]
        public byte AttackCapability { get; init; }
        [JsonPropertyName("role_levels")]
        public int[] RoleLevels { get; init; }

        // Stats
        [JsonPropertyName("damage_min")]
        public byte DamageMin { get; init; }
        [JsonPropertyName("damage_max")]
        public byte DamageMax { get; init; }
        [JsonPropertyName("attack_rate")]
        public float AttackRate { get; init; }
        [JsonPropertyName("attack_range")]
        public short AttackRange { get; init; }
        [JsonPropertyName("projectile_speed")]
        public short ProjectileSpeed { get; init; }
        [JsonPropertyName("armor")]
        public float Armour { get; init; }
        [JsonPropertyName("magic_resistance")]
        public byte MagicResistance { get; init; }
        [JsonPropertyName("movement_speed")]
        public short MoveSpeed { get; init; }
        [JsonPropertyName("turn_rate")]
        public float TurnRate { get; init; }
        [JsonPropertyName("sight_range_day")]
        public short ViewRangeDay { get; init; }
        [JsonPropertyName("sight_range_night")]
        public short ViewRangeNight { get; init; }
        [JsonPropertyName("max_health")]
        public short HealthMax { get; init; }
        [JsonPropertyName("health_regen")]
        public float HealthRegen { get; init; }
        [JsonPropertyName("max_mana")]
        public short ManaMax { get; init; }
        [JsonPropertyName("mana_regen")]
        public float ManaRegen { get; init; }


        // Is there anything we don't know?
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

        public int GetRoleLevel(Role role)
            => RoleLevels[(int)role];

        public Role[] GetHightestRoles()
            => RoleLevels.Select((value, index) => new { value, index }).Where(x => x.value == RoleLevels.Max()).Select(x => (Role)x.index).ToArray();

        public Role[] GetAllRoles()
            => RoleLevels.Select((value, index) => new { value, index }).Where(x => x.value != 0).Select(x => (Role)x.index).ToArray();

        public AttackType GetAttackType()
            => AttackCapability == 1 ? AttackType.Melee : AttackType.Ranged;
    }

    public enum PrimaryAttribute
    {
        Strength,
        Agility,
        Intelligence
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

    public enum AttackType
    {
        Melee,
        Ranged
    }
}
