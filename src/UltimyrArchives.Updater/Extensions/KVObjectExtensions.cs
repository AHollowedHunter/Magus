using System.Diagnostics.Contracts;
using ValveKeyValue;

namespace UltimyrArchives.Updater.Extensions;

public static class KVObjectExtensions
{
    [Pure]
    public static KVValue GetRequiredValue(this KVObject kvObject, string key)
        => kvObject[key] ?? throw new KeyNotFoundException(key);

    [Pure]
    public static string GetRequiredString(this KVObject kvObject, string key, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToString(provider);

    [Pure]
    public static bool GetRequiredBoolean(this KVObject kvObject, string key, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToBoolean(provider);

    [Pure]
    public static byte GetRequiredByte(this KVObject kvObject, string key, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToByte(provider);

    [Pure]
    public static DateTime GetRequiredDateTime(this KVObject kvObject, string key, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToDateTime(provider);

    [Pure]
    public static decimal GetRequiredDecimal(this KVObject kvObject, string key, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToDecimal(provider);

    [Pure]
    public static double GetRequiredDouble(this KVObject kvObject, string key, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToDouble(provider);

    [Pure]
    public static short GetRequiredInt16(this KVObject kvObject, string key, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToInt16(provider);

    [Pure]
    public static int GetRequiredInt32(this KVObject kvObject, string key, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToInt32(provider);

    [Pure]
    public static long GetRequiredInt64(this KVObject kvObject, string key, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToInt64(provider);

    [Pure]
    public static sbyte GetRequiredSByte(this KVObject kvObject, string key, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToSByte(provider);

    [Pure]
    public static float GetRequiredSingle(this KVObject kvObject, string key, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToSingle(provider);

    [Pure]
    public static object GetRequiredType(this KVObject kvObject, string key, Type conversionType, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToType(conversionType, provider);

    [Pure]
    public static ushort GetRequiredUInt16(this KVObject kvObject, string key, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToUInt16(provider);

    [Pure]
    public static uint GetRequiredUInt32(this KVObject kvObject, string key, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToUInt32(provider);

    [Pure]
    public static ulong GetRequiredUInt64(this KVObject kvObject, string key, IFormatProvider? provider = null)
        => kvObject.GetRequiredValue(key).ToUInt32(provider);
}
