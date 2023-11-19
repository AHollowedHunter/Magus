using Discord;

namespace Magus.Bot.Extensions;

public static class EmbedExtensions
{
    public static Discord.Embed CreateDiscordEmbed(this Data.Models.Embeds.Embed embed)
    {
        var discordEmbed = new EmbedBuilder()
        {
            Title        = embed.Title,
            Description  = embed.Description,
            Url          = embed.Url,
            ImageUrl     = embed.ImageUrl,
            ThumbnailUrl = embed.ThumbnailUrl,
            Color        = embed.ColorRaw,
            Timestamp    = embed.Timestamp,
        };

        if (embed.Fields != null && embed.Fields.Any())
            foreach (var field in embed.Fields)
                discordEmbed.AddField(field.Name, field.Value, field.IsInline);

        if (embed.Footer != null)
            discordEmbed.Footer = new() { Text = embed.Footer?.Text, IconUrl = embed.Footer?.IconUrl };

        return discordEmbed.Build();
    }
}
