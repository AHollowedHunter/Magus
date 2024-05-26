namespace Magus.Common.Dota.Enums;

[Flags]
public enum DamageFlag
{
    NONE                                         = 0,
    IGNORES_MAGIC_ARMOR                          = 1,
    IGNORES_PHYSICAL_ARMOR                       = 2,
    BYPASSES_INVULNERABILITY                     = 4,
    BYPASSES_BLOCK                               = 8,
    REFLECTION                                   = 16,
    HPLOSS                                       = 32,
    NO_DIRECTOR_EVENT                            = 64,
    NON_LETHAL                                   = 128,
    USE_COMBAT_PROFICIENCY                       = 256,
    NO_DAMAGE_MULTIPLIERS                        = 512,
    NO_SPELL_AMPLIFICATION                       = 1024,
    DONT_DISPLAY_DAMAGE_IF_SOURCE_HIDDEN         = 2048,
    NO_SPELL_LIFESTEAL                           = 4096,
    PROPERTY_FIRE                                = 8192,
    IGNORES_BASE_PHYSICAL_ARMOR                  = 16384,
    DOTA_DAMAGE_FLAG_SECONDARY_PROJECTILE_ATTACK = 32768,
    DOTA_DAMAGE_FLAG_FORCE_SPELL_AMPLIFICATION   = 65536,
    DOTA_DAMAGE_FLAG_MAGIC_AUTO_ATTACK           = 131072,
    DOTA_DAMAGE_FLAG_ATTACK_MODIFIER             = 262144,
}
