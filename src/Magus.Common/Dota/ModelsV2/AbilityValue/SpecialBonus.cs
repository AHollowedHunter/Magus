namespace Magus.Common.Dota.ModelsV2.AbilityValue;

public class SpecialBonus(string name, SpecialBonusValue value)
{
    public string Name { get; set; } = name;

    public SpecialBonusValue Value { get; set; } = value;
}
