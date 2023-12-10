using System.Text.Json.Serialization;

namespace Magus.Data.Models.V2;
public sealed class Patch : IPatch
{
    public Patch(string patchNumber, ulong timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(patchNumber);

        PatchNumber = patchNumber;
        Timestamp = timestamp;
    }

    [JsonPropertyName(nameof(PatchNumber))]
    public string PatchNumber { get; set; }

    [JsonPropertyName(nameof(Timestamp))]
    public ulong Timestamp { get; set; }
}
