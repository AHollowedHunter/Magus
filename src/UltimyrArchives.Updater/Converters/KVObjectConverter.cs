using System.Diagnostics.Contracts;

namespace UltimyrArchives.Updater.Converters;

public abstract class KVObjectConverter
{
    [Pure]
    protected static T[] ConvertList<T>(KVObject? kvObject, Func<KVOPair, T> converter)
    {
        if (kvObject is null or { Count: 0 })
            return [];
        
        if (kvObject.ValueType is KVValueType.String)
            return []; // TODO sort this
        
        var values = new T[kvObject.Count];
        var index  = 0;
        foreach (var value in kvObject)
            values[index++] = converter(value);

        return values;
    }
}
