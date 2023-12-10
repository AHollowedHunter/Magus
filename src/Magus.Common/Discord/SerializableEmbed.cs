using Discord;
using System.Text.Json.Serialization;

namespace Magus.Common.Discord;

public sealed class SerializableEmbed
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageUrl { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ThumbnailUrl { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint? ColorRaw { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<SerializableField>? Fields { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SerializableFooter? Footer { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? Timestamp { get; set; }

    public Embed ToDiscordEmbed()
    {
        var discordEmbed = new EmbedBuilder()
        {
            Title        = Title,
            Description  = Description,
            Url          = Url,
            ImageUrl     = ImageUrl,
            ThumbnailUrl = ThumbnailUrl,
            Color        = ColorRaw,
            Timestamp    = Timestamp,
        };

        if (Fields is not null && Fields.Any())
            foreach (var field in Fields)
                discordEmbed.AddField(field.Name, field.Value, field.IsInline);

        if (Footer is not null)
            discordEmbed.Footer = new() { Text = Footer?.Text, IconUrl = Footer?.IconUrl };

        return discordEmbed.Build();
    }

    public static SerializableEmbed FromDiscordEmbed(Embed discordEmbed)
    {
        return new SerializableEmbed()
        {
            Title = discordEmbed.Title,
            Description = discordEmbed.Description,
            Url = discordEmbed.Url,
            ImageUrl = discordEmbed.Image?.Url,
            ThumbnailUrl = discordEmbed.Image?.Url,
            ColorRaw = discordEmbed.Color?.RawValue,
            Timestamp = discordEmbed.Timestamp,
            Footer = discordEmbed.Footer.HasValue ? new SerializableFooter(discordEmbed.Footer.Value.Text, discordEmbed.Footer?.IconUrl) : null,
            Fields = discordEmbed.Fields.Any() ? discordEmbed.Fields.Select(f => new SerializableField(f.Name, f.Value, f.Inline)) : null,
        };
    }
}
