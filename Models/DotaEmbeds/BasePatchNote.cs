namespace Magus.Data.Models.DotaEmbeds
{
    public abstract record BasePatchNote : BaseRecord
    {
        public string PatchNumber { get; init; }
        public Embed Embed { get; init; }
    }
}
