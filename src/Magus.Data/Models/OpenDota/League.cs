using System.Text.Json.Serialization;

namespace Magus.Data.Models.OpenDota;

public sealed record League
{
    [JsonPropertyName("leagueid")]
    public long Leagueid { get; set; }
    [JsonPropertyName("ticket")]
    public string Ticket { get; set; }
    [JsonPropertyName("banner")]
    public string Banner { get; set; }
    [JsonPropertyName("tier")]
    public string Tier { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
