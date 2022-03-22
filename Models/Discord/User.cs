namespace Magus.Data.Models.Discord
{
    public record User : SnowflakeRecord
    {
        public string UserName { get; set; }
        /// <summary>
        /// See https://discord.com/developers/docs/reference#locales
        /// </summary>
        public string? Locale { get; set; }

        public long? SteamId { get; set; }
    }
}
