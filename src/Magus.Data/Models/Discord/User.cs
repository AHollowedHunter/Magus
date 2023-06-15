namespace Magus.Data.Models.Discord
{
    public record User : ISnowflakeRecord
    {
        public User() { }
        public User(ulong id) => Id = id;

        /// <summary>
        /// User's Discord ID
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// The set steam account ID, aka DotaID, Friend ID.
        /// </summary>
        public long? DotaID { get; set; }

        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    }
}
