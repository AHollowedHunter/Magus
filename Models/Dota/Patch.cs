using System.Text.Json.Serialization;

namespace Magus.Data.Models.Dota
{
    public record Patch : ISnowflakeRecord
    {
        public ulong Id { get; set; }
        [JsonPropertyName("patch_number")]
        public string PatchNumber { get; init; }
        [JsonPropertyName("patch_timestamp")]
        public int PatchTimestamp { get; init; }
    }
}
