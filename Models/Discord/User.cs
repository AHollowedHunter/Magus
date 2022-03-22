namespace Magus.Data.Models.Discord
{
    public record User : SnowflakeRecord
    {
        public string UserName { get; set; }
        /// <summary>
        /// See <see href="https://discord.com/developers/docs/reference#locales"></see>
        /// </summary>
        public string? Locale { get; set; }

        public long? SteamId { get; set; }
    }
}
