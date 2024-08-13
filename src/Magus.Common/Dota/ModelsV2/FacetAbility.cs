using System.Diagnostics;

namespace Magus.Common.Dota.ModelsV2;

[DebuggerDisplay("{AbilityName}")]
public class FacetAbility
{
    public required string AbilityName { get; init; }
    
    public int? AbilityIndex { get; init; } // 0-base
    
    public bool AutoLevelAbility { get; init; }
    
    public string? ReplaceAbility { get; init; } // e.g. Disruptor
}
