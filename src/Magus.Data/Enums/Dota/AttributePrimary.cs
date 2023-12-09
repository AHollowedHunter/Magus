using System.ComponentModel.DataAnnotations;

namespace Magus.Data.Enums.Dota;

public enum AttributePrimary
{
    [Display(Name = "Strength")]
    DOTA_ATTRIBUTE_STRENGTH,
    [Display(Name = "Agility")]
    DOTA_ATTRIBUTE_AGILITY,
    [Display(Name = "Intelligence")]
    DOTA_ATTRIBUTE_INTELLECT,
    [Display(Name = "Universal")]
    DOTA_ATTRIBUTE_ALL
}
