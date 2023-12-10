namespace Magus.Data.Models.Dota;
 // TODO (re?)move this class
public record Patch : ISnowflakeId
{
    public ulong Id { get; set; }
    public string PatchNumber { get; init; }
    public ulong Timestamp { get; init; }
}
