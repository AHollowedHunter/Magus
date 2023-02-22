using Discord;

namespace Magus.Bot.Extensions
{
    public static class EmbedExtensions
    {
        public static Discord.Embed CreateDiscordEmbed(this Data.Models.Embeds.Embed embed)
        {
            var discordEmbed = new EmbedBuilder()
            {
                Title = embed.Title,
                Description = embed.Description,
                Url = embed.Url,
                ImageUrl = embed.ImageUrl,
                ThumbnailUrl = embed.ThumbnailUrl,
                Color = embed.ColorRaw,
                Timestamp = embed.Timestamp,
            };

            if (embed.Fields != null && embed.Fields.Any())
            {
                var fields = new List<EmbedFieldBuilder>();
                foreach (var field in embed.Fields)
                {
                    fields.Add(new EmbedFieldBuilder() { Name = field.Name, IsInline = field.IsInline, Value = field.Value });
                }
                discordEmbed.Fields = fields;
            }

            if (embed.Footer != null)
            {
                discordEmbed.Footer = new() { Text = embed.Footer?.Text, IconUrl = embed.Footer?.IconUrl };
            }

            return discordEmbed.Build();
        }
    }
}
