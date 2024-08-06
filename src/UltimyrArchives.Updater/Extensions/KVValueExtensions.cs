using System.Diagnostics.Contracts;
using System.Globalization;
using ValveKeyValue;

namespace UltimyrArchives.Updater.Extensions;

public static class KVValueExtensions
{
    [Pure]
    public static IEnumerable<KVObject>? AsEnumerable(this KVValue kvValue)
        => kvValue as IEnumerable<KVObject>;

    [Pure]
    public static T ToEnum<T>(this KVValue? kvValue) where T : struct, Enum
    {
        if (kvValue is null)
            return default;
        Enum.TryParse<T>(kvValue.ToString(CultureInfo.InvariantCulture).Replace('|', ','), true, out var result);
        return result;
    }
}
