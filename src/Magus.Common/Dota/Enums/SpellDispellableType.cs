using System.ComponentModel.DataAnnotations;

namespace Magus.Common.Dota.Enums;

public enum SpellDispellableType : byte
{
    SPELL_DISPELLABLE_NONE       = 0,
    [Display(Name = "Strong Dispels")] // Localise DOTA_ToolTip_Dispellable
    SPELL_DISPELLABLE_YES_STRONG = 1,
    [Display(Name = "Basic Dispel")]
    SPELL_DISPELLABLE_YES        = 2,
    [Display(Name = "No")]
    SPELL_DISPELLABLE_NO         = 3,
}
