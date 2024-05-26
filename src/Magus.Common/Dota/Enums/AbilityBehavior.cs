﻿using System.ComponentModel.DataAnnotations;

namespace Magus.Common.Dota.Enums;

[Flags]
public enum AbilityBehavior : ulong
{
    DOTA_ABILITY_BEHAVIOR_NONE                           = 0,
    DOTA_ABILITY_BEHAVIOR_HIDDEN                         = 1,
    [Display(Name                                        = "Passive")]
    DOTA_ABILITY_BEHAVIOR_PASSIVE                        = 2,
    [Display(Name                                        = "No Target")]
    DOTA_ABILITY_BEHAVIOR_NO_TARGET                      = 4,
    [Display(Name                                        = "Unit Target")]
    DOTA_ABILITY_BEHAVIOR_UNIT_TARGET                    = 8,
    [Display(Name                                        = "Point target")]
    DOTA_ABILITY_BEHAVIOR_POINT                          = 16,
    [Display(Name                                        = "AOE")]
    DOTA_ABILITY_BEHAVIOR_AOE                            = 32,
    DOTA_ABILITY_BEHAVIOR_NOT_LEARNABLE                  = 64,
    DOTA_ABILITY_BEHAVIOR_CHANNELLED                     = 128,
    DOTA_ABILITY_BEHAVIOR_ITEM                           = 256,
    DOTA_ABILITY_BEHAVIOR_TOGGLE                         = 512,
    DOTA_ABILITY_BEHAVIOR_DIRECTIONAL                    = 1024,
    DOTA_ABILITY_BEHAVIOR_IMMEDIATE                      = 2048,
    DOTA_ABILITY_BEHAVIOR_AUTOCAST                       = 4096,
    DOTA_ABILITY_BEHAVIOR_OPTIONAL_UNIT_TARGET           = 8192,
    DOTA_ABILITY_BEHAVIOR_OPTIONAL_POINT                 = 16384,
    DOTA_ABILITY_BEHAVIOR_OPTIONAL_NO_TARGET             = 32768,
    DOTA_ABILITY_BEHAVIOR_AURA                           = 65536,
    DOTA_ABILITY_BEHAVIOR_ATTACK                         = 131072,
    DOTA_ABILITY_BEHAVIOR_DONT_RESUME_MOVEMENT           = 262144,
    DOTA_ABILITY_BEHAVIOR_ROOT_DISABLES                  = 524288,
    DOTA_ABILITY_BEHAVIOR_UNRESTRICTED                   = 1048576,
    DOTA_ABILITY_BEHAVIOR_IGNORE_PSEUDO_QUEUE            = 2097152,
    DOTA_ABILITY_BEHAVIOR_IGNORE_CHANNEL                 = 4194304,
    DOTA_ABILITY_BEHAVIOR_DONT_CANCEL_MOVEMENT           = 8388608,
    DOTA_ABILITY_BEHAVIOR_DONT_ALERT_TARGET              = 16777216,
    DOTA_ABILITY_BEHAVIOR_DONT_RESUME_ATTACK             = 33554432,
    DOTA_ABILITY_BEHAVIOR_NORMAL_WHEN_STOLEN             = 67108864,
    DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING               = 134217728,
    DOTA_ABILITY_BEHAVIOR_RUNE_TARGET                    = 268435456,
    DOTA_ABILITY_BEHAVIOR_DONT_CANCEL_CHANNEL            = 536870912,
    DOTA_ABILITY_BEHAVIOR_VECTOR_TARGETING               = 1073741824,
    DOTA_ABILITY_BEHAVIOR_LAST_RESORT_POINT              = 2147483648,
    DOTA_ABILITY_BEHAVIOR_CAN_SELF_CAST                  = 4294967296,
    DOTA_ABILITY_BEHAVIOR_SHOW_IN_GUIDES                 = 8589934592,
    DOTA_ABILITY_BEHAVIOR_UNLOCKED_BY_EFFECT_INDEX       = 17179869184,
    DOTA_ABILITY_BEHAVIOR_SUPPRESS_ASSOCIATED_CONSUMABLE = 34359738368,
    DOTA_ABILITY_BEHAVIOR_FREE_DRAW_TARGETING            = 68719476736,
    DOTA_ABILITY_BEHAVIOR_IGNORE_SILENCE                 = 137438953472,
    DOTA_ABILITY_BEHAVIOR_OVERSHOOT                      = 274877906944,
    DOTA_ABILITY_BEHAVIOR_IGNORE_MUTED                   = 549755813888,
    DOTA_ABILITY_BEHAVIOR_ALT_CASTABLE                   = 2^40,
    DOTA_ABILITY_BEHAVIOR_BREAK_DISABLES                 = 2^41,
    DOTA_ABILITY_BEHAVIOR_SKIP_FOR_KEYBINDS              = 2^42,
    DOTA_ABILITY_BEHAVIOR_INNATE_UI                      = 2^43,
}
