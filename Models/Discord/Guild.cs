namespace Magus.Data.Models.Discord
{
    public record Guild : SnowflakeRecord
    {
        public string Name { get; set; }
        public ulong OwnerId { get; set; }
        public DateTimeOffset JoinedAt { get; init; }
        public bool? Large { get; set; }
        public int MemberCount { get; set; }
        /// <summary>
        /// See https://discord.com/developers/docs/reference#locales
        /// </summary>
        public string PreferredLocale { get; set; }
    }
}
