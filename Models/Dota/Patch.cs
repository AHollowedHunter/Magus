namespace Magus.Data.Models.Dota
{
    public record Patch : IGuidRecord
    {
        public Guid Id { get; set; }
        public string PatchNumber { get; init; }
        public int PatchTimestamp { get; init; }
        public string? PatchWebsite { get; init; }
    }
}
