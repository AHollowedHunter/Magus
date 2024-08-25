using System.Globalization;

namespace Magus.Common.Dota.ModelsV2.AbilityValue;

public readonly struct SpecialBonusValue(float value, bool isEquals = false, bool isMultiplier = false, bool isPercent = false)
{
    public float Value { get; } = value;

    public bool IsEquals { get; } = isEquals;

    public bool IsMultiplier { get; } = isMultiplier;

    public bool IsPercent { get; } = isPercent;

    public static SpecialBonusValue[] Parse(string[] values)
    {
        var results = new SpecialBonusValue[values.Length];
        for (int i = 0; i < values.Length; i++)
            results[i] = SpecialBonusValue.Parse(values[i]);
        return results;
    }

    public static SpecialBonusValue Parse(string value)
    {
        if (value.Length <= 1)
            return new SpecialBonusValue(float.Parse(value));

        // e.g. pudge_rot
        if (value[..2] == "+-") // Why are you the way that you are?
        {
            value = value[1..];
        }

        bool isEquals = false;
        if (value[0] == '=')
        {
            isEquals = true;
            value    = value[1..];
        }

        bool isMultiplier = false;
        if (value[0] == 'x')
        {
            isMultiplier = true;
            value        = value[1..];
        }

        if (value[^1] == '%')
            return new SpecialBonusValue(float.Parse(value[..^1]), isEquals, isMultiplier, true);

        return new SpecialBonusValue(float.Parse(value), isEquals, isMultiplier, false);
    }
}
