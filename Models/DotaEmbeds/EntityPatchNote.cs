namespace Magus.Data.Models.DotaEmbeds
{
    public abstract record EntityPatchNote : BasePatchNote
    {
        public int EntityId { get; init; }
        public string? InternalName { get; init; }
        public string? LocalName { get; init; }
        public IEnumerable<string>? Aliases { get; init; }
    }
}
