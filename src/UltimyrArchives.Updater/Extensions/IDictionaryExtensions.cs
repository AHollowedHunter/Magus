namespace UltimyrArchives.Updater.Extensions;

public static class IDictionaryExtensions
{
    extension<TKey, TEntity>(IDictionary<TKey, TEntity> dictionary)
    {
        public void AddRange(IDictionary<TKey, TEntity> source)
        {
            foreach (var item in source)
                dictionary.Add(item.Key, item.Value);
        }
    }
}
