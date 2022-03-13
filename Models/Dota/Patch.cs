using System.Text.Json.Serialization;

namespace Magus.Data.Models.Dota
{
    public record Patch
    {
        [JsonPropertyName("patch_number")]
        public string PatchNumber { get; init; }
        [JsonPropertyName("patch_name")]
        public string PatchName { get; init; }
        [JsonPropertyName("patch_timestamp")]
        public int PatchTimestamp { get; init; }
        [JsonPropertyName("patch_website")]
        public string? PatchWebsite { get; init; }
        [JsonPropertyName("patch_website_anchor")]
        public string? PatchWebsiteAnchor { get; init; }
    }
}
