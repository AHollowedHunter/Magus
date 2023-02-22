namespace Magus.Data.Models.Discord
{
    public record Guild : ISnowflakeRecord
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public ulong OwnerId { get; set; }
        public DateTimeOffset JoinedAt { get; init; }
        public bool? Large { get; set; }
        public int MemberCount { get; set; }
        /// <summary>
        /// See <see href="https://discord.com/developers/docs/reference#locales"></see>
        /// </summary>
        public string PreferredLocale { get; set; }
    }
}
