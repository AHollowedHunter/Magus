using System.ComponentModel.DataAnnotations;

namespace Magus.Common.Dota.Enums;

[Flags]
public enum AbilityUnitTargetTeam : byte
{
    DOTA_UNIT_TARGET_TEAM_NONE     = 0,
    [Display(Name = "Allies")]
    DOTA_UNIT_TARGET_TEAM_FRIENDLY = 1,
    [Display(Name = "Enemy")]
    DOTA_UNIT_TARGET_TEAM_ENEMY    = 2,
    [Display(Name = "Both Teams")]
    DOTA_UNIT_TARGET_TEAM_BOTH     = 3,
    DOTA_UNIT_TARGET_TEAM_CUSTOM   = 4,
    ENEMY_CUSTOM                   = 6, // HACK for Harpoon, what else?
    FORCEABILITY                   = 7, // Friendly, enemy, and custom
}
