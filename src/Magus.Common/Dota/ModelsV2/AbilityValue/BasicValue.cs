using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Magus.Common.Dota.ModelsV2.AbilityValue;

public record BasicValue : IAbilityValue
{
    public static readonly string[] Keys = ["value", "affected_by_aoe_increase"];
    public BasicValue() { } // For inheritance

    [SetsRequiredMembers]
    public BasicValue(string name, float[] value, bool affectedByAOEIncrease = false)
    {
        Name                  = name;
        Value                 = value;
        AffectedByAOEIncrease = affectedByAOEIncrease;
    }

    public required string Name { get; init; }

    public required float[] Value { get; init; }

    [DataMember(Name = "affected_by_aoe_increase")]
    public bool AffectedByAOEIncrease { get; init; } // affected_by_aoe_increase
}
