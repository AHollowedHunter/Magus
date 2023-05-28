namespace Magus.Data.Models.Discord
{
    public record User : ISnowflakeRecord
    {
        public User() { }
        public User(ulong id) => Id = id;

        public ulong Id { get; set; }

        public uint? DotaID { get; set; }
        public ulong? SteamID64 { get; set; }

        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    }
}
