namespace Magus.Data.Models.Embeds
{
    public abstract record EntityPatchNoteEmbed : BasePatchNoteEmbed, INamedEntity
    {
        public int EntityId { get; set; }
        public string InternalName { get; set; }
        public string Name { get; set; }
        public string? RealName { get; set; }
        public IEnumerable<string>? Aliases { get; set; }
    }
}
