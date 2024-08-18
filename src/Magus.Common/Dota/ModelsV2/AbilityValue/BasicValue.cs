using System.Runtime.Serialization;

namespace Magus.Common.Dota.ModelsV2.AbilityValue;

public class BasicValue : IAbilityValue
{
    public required string Name { get; set; }

    public required float[] Value { get; set; }

    [DataMember(Name = "affected_by_aoe_increase")]
    public bool AffectedByAOEIncrease { get; set; } // affected_by_aoe_increase
}
