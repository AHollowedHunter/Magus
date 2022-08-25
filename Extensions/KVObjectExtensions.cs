using System.Text.RegularExpressions;
using ValveKeyValue;

namespace Magus.DataBuilder.Extensions
{
    public static class KVObjectExtensions
    {
        public static T ParseEnum<T>(this KVObject? kvObject) where T : struct
        {
            if (kvObject == null)
                return default(T);

            var value = kvObject.Value.ToString() ?? "";
            value = value.Replace("|", ",");
            Enum.TryParse<T>(value, true, out var result);
            return result;
        }

        public static IEnumerable<T> ParseList<T>(this KVObject? kvObject) where T : IConvertible
        {
            List<T> result = new();
            if (kvObject == null)
                return result;

            var value = kvObject.Value.ToString() ?? "";
            var separators = new Regex("[,;\\s]+");
            var values = separators.Split(value);
            result.AddRange(values.Select(x => (T)Convert.ChangeType(x, typeof(T))).ToList());

            return result;
        }

        public static T? ParseValue<T>(this KVObject? kvObject) where T : IConvertible
            => (T)Convert.ChangeType(kvObject?.Value!, typeof(T));
    }
}
