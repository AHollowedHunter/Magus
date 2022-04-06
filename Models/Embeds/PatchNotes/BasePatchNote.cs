namespace Magus.Data.Models.Embeds
{
    public abstract record BasePatchNote : IGuidRecord
    {
        public Guid Id { get; set; }
        public string PatchNumber { get; init; }
        public Embed Embed { get; init; }
        public DateTimeOffset TimeStamp { get; init; }
    }
}
