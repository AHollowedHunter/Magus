using System.ComponentModel.DataAnnotations;

namespace Magus.Common.Dota.Enums;

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
