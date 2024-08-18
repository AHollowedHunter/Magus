namespace Magus.Common.Dota.ModelsV2.AbilityValue;

public interface IAbilityValue
{
    string Name { get; }

    float[] Value { get; }

    bool AffectedByAOEIncrease { get; set; }
}
