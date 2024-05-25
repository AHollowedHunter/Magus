using System.Text.Json.Serialization;

namespace Magus.Data.Models.Dota;
public sealed class Patch : IPatch
{
    public Patch(string uniqueId, string patchNumber, ulong timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(patchNumber);

        UniqueId = uniqueId;
        PatchNumber = patchNumber;
        Timestamp = timestamp;
    }

    [JsonPropertyName(nameof(UniqueId))]
    public string UniqueId { get; set; }

    [JsonPropertyName(nameof(PatchNumber))]
    public string PatchNumber { get; set; }

    [JsonPropertyName(nameof(Timestamp))]
    public ulong Timestamp { get; set; }
}
