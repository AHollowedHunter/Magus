namespace Magus.Data.Enums.Dota;

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
