using System.Diagnostics.Contracts;
using UltimyrArchives.Updater.Extensions;

namespace UltimyrArchives.Updater.Converters;

public abstract class KVObjectConverter
{
    [Pure]
    protected static T[] ConvertList<T>(KVValue? kvValue, Func<KVObject, T> converter)
    {
        if (kvValue?.CastEnumerable().ToArray() is not { Length: > 0 } kvObjects)
            return [];

        var values = new T[kvObjects.Length];
        var index  = 0;
        foreach (var value in kvObjects)
            values[index++] = converter(value);

        return values;
    }

    [Pure]
    protected static T[] ConvertList<T>(KVObject[] kvObjects, Func<KVObject, T> converter)
    {
        var values = new T[kvObjects.Length];
        var index  = 0;
        foreach (var value in kvObjects)
            values[index++] = converter(value);

        return values;
    }
}
