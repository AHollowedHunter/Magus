using System.Text.Json.Serialization;

namespace Magus.Common.Discord;

public sealed class SerializableFooter
{
    public SerializableFooter(string text, string? iconUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        Text = text;
        IconUrl = iconUrl;
    }

    [JsonPropertyName(nameof(Text))]
    public string Text { get; set; }

    [JsonPropertyName(nameof(IconUrl))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IconUrl { get; set; }

    public static implicit operator SerializableFooter(string text) => new(text);
}
