namespace Magus.Data.Models.Embeds
{
    public abstract record BasePatchNote : ISnowflakeRecord
    {
        public ulong Id { get; set; }
        public string PatchNumber { get; init; }
        public Embed Embed { get; init; }
        public DateTimeOffset TimeStamp { get; init; }
    }
}
