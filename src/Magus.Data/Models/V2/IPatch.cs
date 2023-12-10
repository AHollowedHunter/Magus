namespace Magus.Data.Models.V2;
public interface IPatch
{
    public string PatchNumber { get; }
    public ulong Timestamp { get; }
}
