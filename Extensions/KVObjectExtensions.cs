using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using ValveKeyValue;

namespace Magus.DataBuilder.Extensions
{
    public static class KVObjectExtensions
    {

        /// <summary>
        /// Use for a single key value when expected
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="kvObject"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T? ParseChildValue<T>(this KVObject kvObject, string name, T defaultValue = default) where T : IConvertible
        {
            var child = kvObject.Children.FirstOrDefault(x => x.Name == name);
            if (child == null)
                return defaultValue;
            return child.ParseValue<T>();
        }

        /// <summary>
        /// Use for a single key value when expected
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="kvObject"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TEnum ParseChildEnum<TEnum>(this KVObject kvObject, string name) where TEnum : struct, Enum
        {
            var child = kvObject.Children.FirstOrDefault(x => x.Name == name);
            if (child == null)
                return default;
            return child.ParseEnum<TEnum>();
        }

        /// <summary>
        /// Use for a list of values when expected
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="kvObject"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static IList<T> ParseChildValueList<T>(this KVObject kvObject, string name, IEnumerable<T> defaultValue = null!) where T : IConvertible
        {
            var child = kvObject.Children.FirstOrDefault(x => x.Name == name);
            if (child == null)  
                return Array.Empty<T>();

            return child.ParseList<T>();
        }

        /// <summary>
        /// Use for a list of values when expected
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="kvObject"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static IList<TEnum> ParseChildEnumList<TEnum>(this KVObject kvObject, string name) where TEnum : struct, Enum
        {
            var child = kvObject.Children.FirstOrDefault(x => x.Name == name);
            if (child == null)  
                return Array.Empty<TEnum>();

            return child.ParseEnumList<TEnum>();
        }

        public static IList<T> ParseList<T>(this KVObject? kvObject) where T : IConvertible
        {
            List<T> result = new();
            if (kvObject == null)
                return result;

            var kvValue    = kvObject.Value.ToString() ?? "";
            var separators = new Regex("[,;\\s]+");
            var values     = separators.Split(kvValue);

            foreach (var value in values)
            {
                if (string.IsNullOrEmpty(value)) continue;
                result.Add(ParseValue<T>(value));
            }
            return result;
        }

        /// <summary>
        /// Parse a list of enums in the order they were stored
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="kvObject"></param>
        /// <returns></returns>
        public static IList<TEnum> ParseEnumList<TEnum>(this KVObject? kvObject) where TEnum : struct, Enum
        {
            List<TEnum> result = new();
            if (kvObject == null)
                return result;

            var kvValue    = kvObject.Value.ToString() ?? "";
            var separators = new Regex("[,;|\\s]+");
            var values     = separators.Split(kvValue);

            foreach (var value in values)
            {
                result.Add(ParseEnum<TEnum>(value));
            }
            return result;
        }

        public static T? ParseValue<T>(this KVObject? kvObject) where T : IConvertible
            => kvObject == null ? default : kvObject.Value.ParseValue<T>();

        public static T ParseValue<T>(this KVValue? obj) where T : IConvertible
            => (T)Convert.ChangeType(obj!, typeof(T));
        

        public static T ParseEnum<T>(this KVObject? kvObject) where T : struct, Enum
            => kvObject?.Value.ParseEnum<T>() ?? default;

        public static T ParseEnum<T>(this KVValue? kvValue) where T : struct, Enum
        {
            if (kvValue == null)
                return default;

            var value = kvValue.ToString() ?? "";
            value     = value.Replace("|", ",");
            Enum.TryParse<T>(value, true, out var result);
            return result;
        }
    }
}
