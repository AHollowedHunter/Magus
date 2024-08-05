using System.Diagnostics.Contracts;
using ValveKeyValue;

namespace UltimyrArchives.Updater.Extensions;

public static class KVValueExtensions
{
    [Pure]
    public static IEnumerable<KVObject>? AsEnumerable(this KVValue kvValue)
        => kvValue as IEnumerable<KVObject>;
}
