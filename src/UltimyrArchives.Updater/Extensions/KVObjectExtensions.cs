using Magus.Common.Extensions;

namespace UltimyrArchives.Updater.Extensions;

public static partial class KVObjectExtensions
{
    extension(KVObject kvObject)
    {
        public (string Key, string Value) KeyValueTupleConverter()
            => (kvObject.Name, kvObject.Value.ToString(CultureInfo.InvariantCulture));
    }
}
