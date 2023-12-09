using System.ComponentModel.DataAnnotations;

namespace Magus.Common.Dota.Enums;

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
