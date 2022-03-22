namespace Magus.Data.Models.Embeds
{
    public abstract record BasePatchNote : GuidRecord
    {
        public string PatchNumber { get; init; }
        public Embed Embed { get; init; }
    }
}
