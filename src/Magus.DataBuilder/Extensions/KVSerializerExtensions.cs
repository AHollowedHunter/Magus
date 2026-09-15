using ValveKeyValue;

namespace Magus.DataBuilder.Extensions;

public static class KVSerializerExtensions
{
    public static async Task<KVObject> GetKVObjectFromLocalUri(this KVSerializer serializer, string uri, string directory, bool hasEscapeSequences = true)
        => await serializer.GetKVObjectFromLocalUri(
            uri,
            new KVSerializerOptions() { HasEscapeSequences = hasEscapeSequences, FileLoader = new DirectoryFileLoader(directory) });

    public static async Task<KVObject> GetKVObjectFromLocalUri(this KVSerializer serializer, string uri, bool hasEscapeSequences = true)
        => await serializer.GetKVObjectFromLocalUri(
            uri,
            new KVSerializerOptions() { HasEscapeSequences = hasEscapeSequences });

    public static async Task<KVObject> GetKVObjectFromLocalUri(this KVSerializer serializer, string uri, KVSerializerOptions options)
    {
        using var stream = new MemoryStream(await File.ReadAllBytesAsync(uri));
        return serializer.Deserialize(stream, options);
    }
}

file class DirectoryFileLoader(string directory) : IIncludedFileLoader
{
    public Stream OpenFile(string filePath)
    {
        return File.OpenRead(Path.Combine(directory, filePath));
    }
}
