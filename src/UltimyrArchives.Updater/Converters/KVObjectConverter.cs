using System.Diagnostics.Contracts;
using UltimyrArchives.Updater.Extensions;
using ValveKeyValue;

namespace UltimyrArchives.Updater.Converters;

public abstract class KVObjectConverter
{
    [Pure]
    protected static T[] ConvertList<T>(KVValue? kvValue, Func<KVObject, T> converter)
    {
        if (kvValue?.CastEnumerable().ToArray() is not { Length: > 0 } kvValues)
            return [];

        var values = new T[kvValues.Length];
        var index  = 0;
        foreach (var value in kvValues)
            values[index++] = converter(value);

        return values;
    }
}
