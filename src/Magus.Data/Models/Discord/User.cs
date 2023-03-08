namespace Magus.Data.Models.Discord
{
    public record User : ISnowflakeRecord
    {
        public ulong Id { get; set; }

        public long? SteamId { get; set; }
    }
}
