using System.Diagnostics;

namespace Magus.Common.Dota.ModelsV2;

[DebuggerDisplay("{InternalName}")]
public class Facet
{
    public required string InternalName { get; init; }

    public required string Icon { get; init; }

    public required string Color { get; init; }

    public int GradientId { get; init; }

    public bool Deprecated { get; init; } // e.g. Wind

    // e.g. Tide, warlock, lina, mirana, marci
    public required FacetAbility[] Abilities { get; init; }

    // e.g Tidehunter
    public required (string Key, string Value)[] KeyValueOverrides { get; init; }

    // e.g. Lycan, chen
    public required (string Original, string Replacement)[] AbilityIconReplacements { get; init; }
}
