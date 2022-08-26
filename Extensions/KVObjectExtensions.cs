using Newtonsoft.Json.Linq;
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

        /// <summary>
        /// Use for single values when expected
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="kvObject"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T? ParseChildValue<T>(this KVObject kvObject, string name, T? defaultValue = default) where T: IConvertible
        {
            var child = kvObject.Children.FirstOrDefault(x => x.Name == name);
            if (child == null)
                return defaultValue;
            return child.ParseValue<T>();
        }

        /// <summary>
        /// Use for a list of values when expected
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="kvObject"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static IEnumerable<T> ParseChildList<T>(this KVObject kvObject, string name, IEnumerable<T> defaultValue = null!) where T: IConvertible
        {
            var child = kvObject.Children.FirstOrDefault(x => x.Name == name);
            if (child == null)
                return Enumerable.Empty<T>();

            return child.ParseList<T>();
        }

        public static IEnumerable<T> ParseList<T>(this KVObject? kvObject) where T : IConvertible
        {
            List<T> result = new();
            if (kvObject == null)
                return result;

            var kvValue    = kvObject.Value.ToString() ?? "";
            var separators = new Regex("[,;\\s]+");
            var values     = separators.Split(kvValue);

            foreach (var value in values)
            {
                result.Add(ParseValue<T>(value) ?? default!);
            }

            return result;
        }

        public static T? ParseValue<T>(this KVObject? kvObject) where T : IConvertible
            => ParseValue<T>(kvObject?.Value);

        private static T? ParseValue<T>(this KVValue? obj) where T : IConvertible
        {
            if (typeof(T).IsEnum)
                return (T)Enum.Parse(typeof(T), ParseValue<string>(obj)!);
            return (T)Convert.ChangeType(obj!, typeof(T));
        }
    }
}
