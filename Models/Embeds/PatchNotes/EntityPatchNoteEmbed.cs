namespace Magus.Data.Models.Embeds
{
    public abstract record EntityPatchNoteEmbed : BasePatchNoteEmbed
    {
        public int EntityId { get; init; }
        public string? InternalName { get; init; }
        public string? RealName { get; init; }
        public string? LocalName { get; init; }
        public IEnumerable<string>? Aliases { get; init; }
    }
}
