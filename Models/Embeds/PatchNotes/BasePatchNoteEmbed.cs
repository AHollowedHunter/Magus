namespace Magus.Data.Models.Embeds
{
    public abstract record BasePatchNoteEmbed : ISnowflakeRecord, ILocaleRecord
    {
        public ulong Id { get; set; }
        public string Locale { get; set; }
        public string PatchNumber { get; init; }
        public ulong Timestamp { get; set; }
        public Embed Embed { get; init; }
    }
}
