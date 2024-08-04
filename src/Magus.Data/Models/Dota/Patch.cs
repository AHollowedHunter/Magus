using System.Text.Json.Serialization;

namespace Magus.Data.Models.Dota;

public sealed record Patch : IPatch
{
    [JsonConstructor]
    private Patch(string uniqueId, string patchNumber, ulong timestamp)
    {
        UniqueId    = uniqueId;
        PatchNumber = patchNumber;
        Timestamp   = timestamp;
    }

    public Patch(string patchNumber, ulong timestamp) : this(patchNumber.Replace('.', '-'), patchNumber, timestamp)
    {
    }

    [JsonPropertyName(nameof(UniqueId))]
    public string UniqueId { get; init; }

    [JsonPropertyName(nameof(PatchNumber))]
    public string PatchNumber { get; init; }

    [JsonPropertyName(nameof(Timestamp))]
    public ulong Timestamp { get; init; }
}
