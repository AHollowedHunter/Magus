﻿using System.ComponentModel.DataAnnotations;

namespace Magus.Data.Enums.Dota;

public enum SpellImmunityType : byte
{
    SPELL_IMMUNITY_NONE                  = 0,
    [Display(Name = "Allies Yes")]
    SPELL_IMMUNITY_ALLIES_YES            = 1,
    [Display(Name = "No")]
    SPELL_IMMUNITY_ALLIES_NO             = 2,
    [Display(Name = "Yes")]
    SPELL_IMMUNITY_ENEMIES_YES           = 3,
    [Display(Name = "No")]
    SPELL_IMMUNITY_ENEMIES_NO            = 4,
    [Display(Name = "Allies Yes, Enemies No")]
    SPELL_IMMUNITY_ALLIES_YES_ENEMIES_NO = 5,
}
