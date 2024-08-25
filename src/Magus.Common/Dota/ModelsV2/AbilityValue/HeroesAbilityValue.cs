namespace Magus.Common.Dota.ModelsV2.AbilityValue;

public sealed record HeroesAbilityValue : BasicValue
{
    public SpecialBonus[]? SpecialBonuses { get; set; } // for any 'special_bonus_*'

    public bool Innate { get; set; }

    public string? RequiresFacet { get; set; }

    public bool RequiresScepter { get; set; }

    public bool RequiresShard { get; set; }

    // LinkedSpecialBonus - see lina, slardar_slithereen_crush, witch_doctor_maledict
    public string? LinkedSpecialBonus { get; set; }

    public string? LinkedSpecialBonusField { get; set; }

    public string? LinkedSpecialBonusOperation { get; set; }
}
