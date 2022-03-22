namespace Magus.Data.Models.Embeds
{
    public abstract record EntityInfo : GuidRecord
    {
        public int EntityId { get; init; }
        public string? InternalName { get; init; }
        public string? RealName { get; init; }
        public string? LocalName { get; init; }
        public IEnumerable<string>? Aliases { get; init; }
        public Embed Embed { get; init; }
    }
}
