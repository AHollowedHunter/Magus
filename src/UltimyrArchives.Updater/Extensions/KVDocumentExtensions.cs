namespace UltimyrArchives.Updater.Extensions;

public static partial class KVObjectExtensions
{
    extension(KVDocument kvDoc)
    {
        public KVObject GetSingleValue(string key, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return kvDoc.Root.Count is > 0
                ? kvDoc.Root.Children.Single(x => x.Key.Equals(key, stringComparison)).Value
                : throw new KeyNotFoundException();
        }
    }
}
