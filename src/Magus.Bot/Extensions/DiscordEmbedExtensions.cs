using Discord;

namespace Magus.Bot.Extensions
{
    public static class DiscordEmbedExtensions
    {
        public static EmbedBuilder AddEmptyField(this EmbedBuilder builder, bool isInline = false) => builder.AddField("_ _", "_ _", isInline);
    }
}
