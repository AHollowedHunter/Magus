namespace Magus.Data.Models.Embeds
{
    public abstract record EntityInfo : ISnowflakeRecord
    {
        public ulong Id { get; set; }
        public string? InternalName { get; set; }
        public string? RealName { get; set; }
        public string? LocalName { get; set; }
        public IEnumerable<string>? Aliases { get; set; }
        public Embed Embed { get; set; }
    }
}
