namespace Magus.Data.Models.Dota
{
    public record Patch : ISnowflakeRecord
    {
        public ulong Id { get; set; }
        public string PatchNumber { get; init; }
        public ulong Timestamp { get; init; }
    }
}
