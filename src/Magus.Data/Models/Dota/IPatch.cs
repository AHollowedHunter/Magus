namespace Magus.Data.Models.Dota;
public interface IPatch
{
    public string PatchNumber { get; }
    public ulong Timestamp { get; }
}
