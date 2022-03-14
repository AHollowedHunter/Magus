namespace Magus.Data.Models.DotaEmbeds
{
    public record ItemPatchNote : BasePatchNote
    {
        public int ItemId { get; init; }
    }
}
