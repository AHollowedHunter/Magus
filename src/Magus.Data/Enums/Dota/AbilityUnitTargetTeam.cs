using System.ComponentModel.DataAnnotations;

namespace Magus.Data.Enums.Dota;

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
    FORCEABILITY                   = 7, // Friendly, enemy, and custom
}
