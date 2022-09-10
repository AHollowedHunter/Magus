using System.Net.Http.Headers;
using ValveKeyValue;

namespace Magus.DataBuilder.Extensions
{
    public static class KVSerializerExtensions
    {
        public static async Task<KVObject> GetKVObjectFromUri(this KVSerializer serializer, string uri, HttpClient client, bool hasEscapeSequences = true)
        {
            var rawString = await client.GetStringAsync(uri);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(rawString));
            return serializer.Deserialize(stream, new KVSerializerOptions() { HasEscapeSequences = hasEscapeSequences });
        }

        public static async Task<KVObject> GetKVObjectFromUri(this KVSerializer serializer, string uri, HttpClient client, KVSerializerOptions options)
        {
            var rawString = await client.GetStringAsync(uri);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(rawString));
            return serializer.Deserialize(stream, options);
        }
    }
}
