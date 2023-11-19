using System.Text.RegularExpressions;
using ValveKeyValue;

namespace Magus.DataBuilder.Extensions;

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
    public static T? ParseChildValue<T>(this KVObject kvObject, string name, T? defaultValue = default, bool ignoreNoNumeric = false, bool emptyValueReturnDefault = false) where T : IConvertible
    {
        var child = kvObject.Children.FirstOrDefault(x => x.Name == name);
        if (child == null)
            return defaultValue;
        if (emptyValueReturnDefault && string.IsNullOrWhiteSpace(child.Value.ToString()))
            return defaultValue;
        return child.ParseValue<T>(ignoreNoNumeric);
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
    public static IList<T> ParseChildValueList<T>(this KVObject kvObject, string name, bool ignoreNoNumeric = false, IEnumerable<T> defaultValue = null!) where T : IConvertible
    {
        var child = kvObject.Children.FirstOrDefault(x => x.Name == name);
        if (child == null)
            return Array.Empty<T>();

        return child.ParseList<T>(ignoreNoNumeric);
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

    public static IList<T> ParseList<T>(this KVObject? kvObject, bool ignoreNoNumeric = false) where T : IConvertible
    {
        List<T> result = new();
        if (kvObject == null)
            return result;

        var kvValue    = kvObject.Value.ToString() ?? "";
        if (ignoreNoNumeric && IsNumericType(typeof(T)))
        {
            var nonNumeric = new Regex(@"[^\d\-+.,:\s]*");
            kvValue = nonNumeric.Replace(kvValue, "");
        }
        var separators = new Regex(@"[,;\s]+");
        var values     = separators.Split(kvValue);

        foreach (var value in values)
        {
            if (string.IsNullOrEmpty(value)) continue;
            result.Add(ParseValue<T>(value));
        }
        return result;
    }

    private static bool IsNumericType(Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
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

    public static T? ParseValue<T>(this KVObject? kvObject, bool ignoreNoNumeric = false, bool emptyValueReturnDefault = false) where T : IConvertible
    {
        if (kvObject == null)
            return default;
        if (emptyValueReturnDefault && string.IsNullOrWhiteSpace(kvObject.Value.ToString()))
            return default;
        return kvObject.Value.ParseValue<T>(ignoreNoNumeric);
    }

    public static T ParseValue<T>(this KVValue obj, bool ignoreNoNumeric = false) where T : IConvertible
    {
        if (ignoreNoNumeric && IsNumericType(typeof(T)))
        {
            var kvValue = obj?.ToString() ?? "";
            var nonNumeric = new Regex(@"[^\d\-+.,:\s]*");
            kvValue = nonNumeric.Replace(kvValue, "");
            return (T)Convert.ChangeType(kvValue, typeof(T));
        }
        return (T)Convert.ChangeType(obj, typeof(T));
    }


    public static T ParseEnum<T>(this KVObject? kvObject) where T : struct, Enum
        => kvObject?.Value.ParseEnum<T>() ?? default;

    public static T ParseEnum<T>(this KVValue? kvValue) where T : struct, Enum
    {
        if (kvValue == null)
            return default;

        var value = kvValue.ToString() ?? "";
        value = value.Replace("|", ",");
        Enum.TryParse<T>(value, true, out var result);
        return result;
    }
}
