using Magus.Common.Extensions;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text.RegularExpressions;
using ValveKeyValue;

namespace UltimyrArchives.Updater.Extensions;

public static partial class KVValueExtensions
{
    /// <summary>
    /// ValveKeyValue expects booleans as 0/1 and parses int,
    /// but some values are a string version, so...😭
    /// </summary>
    [Pure]
    public static bool ToBoolFromString(this KVValue? value)
        => value?.ToString(CultureInfo.InvariantCulture).Equals("true") ?? false;

    [Pure]
    public static IEnumerable<KVObject>? AsEnumerable(this KVValue kvValue)
        => kvValue as IEnumerable<KVObject>;

    [Pure]
    public static T ToEnum<T>(this KVValue? kvValue) where T : struct, Enum
    {
        if (kvValue is null)
            return default;
        Enum.TryParse<T>(kvValue.ToString(CultureInfo.InvariantCulture).Replace('|', ','), true, out var result);
        return result;
    }

    [Pure]
    public static T[] ParseArray<T>(this KVValue? kvValue, bool spaceIsSeparator = true, bool ignoreNonNumericChars = false) where T : IConvertible
    {
        if (kvValue is null)
            return [];

        string stringValue = kvValue.ToString(CultureInfo.InvariantCulture);
        if (ignoreNonNumericChars && typeof(T).IsNumeric())
            stringValue = NonNumericChars.Replace(stringValue, "");

        var values = spaceIsSeparator ? SeparatorsWithSpace.Split(stringValue) : SeparatorsWithoutSpace.Split(stringValue);
        var converted = from v in values
            where !string.IsNullOrEmpty(v)
            select (T) Convert.ChangeType(v, typeof(T));
        return [..converted];
    }

    [Pure]
    public static T[] ParseEnumArray<T>(this KVValue? kvValue, bool spaceIsSeparator = true, bool ignoreNonNumericChars = false) where T : struct, Enum
    {
        var values = kvValue.ParseArray<string>(spaceIsSeparator, ignoreNonNumericChars);
        var array  = new T[values.Length];
        int index  = 0;
        foreach (var value in values)
            array[index++] = Enum.TryParse<T>(value, true, out var result) ? result : default;
        return array;
    }

    #region Regex

    [GeneratedRegex(@"[,;\s]+")]
    private static partial Regex _SeparatorsWithSpaces(); // TODO change when rider supports partial properties

    private static Regex SeparatorsWithSpace { get => _SeparatorsWithSpaces(); }

    [GeneratedRegex(@"[,;]+")]
    private static partial Regex _SeparatorsWithoutSpace(); // TODO change when rider supports partial properties

    private static Regex SeparatorsWithoutSpace { get => _SeparatorsWithoutSpace(); }

    [GeneratedRegex(@"[^\d\-+.,:\s]*")]
    private static partial Regex _NonNumericChars();

    private static Regex NonNumericChars { get => _NonNumericChars(); }

    #endregion
}
