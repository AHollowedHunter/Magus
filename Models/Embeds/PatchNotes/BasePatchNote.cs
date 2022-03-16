namespace Magus.Data.Models.Embeds
{
    public abstract record BasePatchNote : BaseRecord
    {
        public string PatchNumber { get; init; }
        public Embed Embed { get; init; }
    }
}
