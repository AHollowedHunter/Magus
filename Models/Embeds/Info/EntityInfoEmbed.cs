namespace Magus.Data.Models.Embeds
{
    public abstract record EntityInfoEmbed : ISnowflakeRecord, ILocaleRecord
    {
        public ulong Id { get; set; }
        public int EntityId { get; set; }
        public string Locale { get; set; }
        public string? InternalName { get; set; }
        public string? RealName { get; set; }
        public string? LocalName { get; set; }
        public IEnumerable<string>? Aliases { get; set; }
        public Embed Embed { get; set; }
    }
}
