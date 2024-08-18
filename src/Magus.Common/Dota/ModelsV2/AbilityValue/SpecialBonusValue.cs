namespace Magus.Common.Dota.ModelsV2.AbilityValue;

public readonly struct SpecialBonusValue(float value, bool isEquals, bool isPercent)
{
    public float Value { get; } = value;

    public bool IsEquals { get; } = isEquals;

    public bool IsPercent { get; } = isPercent;

    public static SpecialBonusValue Parse(string value)
    {
        if (value.Length <= 1)
            return new SpecialBonusValue(float.Parse(value), false, false);

        bool isEquals = false;
        if (value[0] == '=')
        {
            isEquals = true;
            value    = value[1..^1];
        }

        if (value[^1] == '%')
            return new SpecialBonusValue(float.Parse(value[..^2]), isEquals, true);

        return new SpecialBonusValue(float.Parse(value), isEquals, false);
    }
}
