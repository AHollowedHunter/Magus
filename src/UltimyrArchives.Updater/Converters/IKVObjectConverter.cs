namespace UltimyrArchives.Updater.Converters;

public interface IKVObjectConverter<out T>
{
    public T Convert(string name, KVObject kvObject);
}
