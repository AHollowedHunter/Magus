namespace Magus.Common.Dota.ModelsV2.AbilityValue;

public class SpecialBonus(string name, SpecialBonusValue[] values)
{
    public string Name { get; set; } = name;

    public SpecialBonusValue[] Values { get; set; } = values;
}
