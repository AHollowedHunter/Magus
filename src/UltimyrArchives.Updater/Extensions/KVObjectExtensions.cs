using Magus.Common.Extensions;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace UltimyrArchives.Updater.Extensions;

public static partial class KVObjectExtensions
{
    extension(KVObject? kvObject)
    {
        [Pure]
        public T[] ParseArray<T>(bool spaceIsSeparator = true, bool ignoreNonNumericChars = false) where T : IConvertible
        {
            if (kvObject is null or { IsArray: true } or { IsCollection: true } or { IsNull: true })
                return [];

            string stringValue = kvObject.ToString(CultureInfo.InvariantCulture);
            if (ignoreNonNumericChars && typeof(T).IsNumeric())
                stringValue = NonNumericChars.Replace(stringValue, "");

            var values = spaceIsSeparator ? SeparatorsWithSpace.Split(stringValue) : SeparatorsWithoutSpace.Split(stringValue);
            var converted = from v in values
                where !string.IsNullOrEmpty(v)
                select (T) Convert.ChangeType(v, typeof(T));
            return [..converted];
        }
    }

    extension(KVObject kvObject)
    {
        [Pure]
        public T GetEnumOrDefault<T>(string key, T defaultValue = default) where T : struct, Enum
            => kvObject.GetValueOrDefault(key)?.ToEnum<T>() ?? defaultValue;

        [Pure]
        public T ToEnum<T>() where T : struct, Enum
        {
            Enum.TryParse<T>(kvObject.ToString(CultureInfo.InvariantCulture).Replace('|', ','), true, out var result);
            return result;
        }


        [Pure]
        public T[] ParseEnumArray<T>(bool spaceIsSeparator = true, bool ignoreNonNumericChars = false) where T : struct, Enum
        {
            var values = kvObject.ParseArray<string>(spaceIsSeparator, ignoreNonNumericChars);
            var array  = new T[values.Length];
            int index  = 0;
            foreach (var value in values)
                array[index++] = Enum.TryParse<T>(value, true, out var result) ? result : default;
            return array;
        }
    }

    #region Regex

    [GeneratedRegex(@"[,;\s]+")]
    private static partial Regex SeparatorsWithSpace { get; }

    [GeneratedRegex(@"[,;]+")]
    private static partial Regex SeparatorsWithoutSpace { get; }

    [GeneratedRegex(@"[^\d\-+.,:\s]*")]
    private static partial Regex NonNumericChars { get; }

    #endregion
}
