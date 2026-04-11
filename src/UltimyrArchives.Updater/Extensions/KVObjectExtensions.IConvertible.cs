using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace UltimyrArchives.Updater.Extensions;

public static partial class KVObjectExtensions
{
    extension(KVObject kvObject)
    {
        [Pure]
        [return: NotNullIfNotNull(nameof(defaultValue))]
        public string? GetStringOrDefault(string key, string? defaultValue = null, IFormatProvider? formatProvider = null)
            => kvObject.GetValueOrDefault(key)?.ToString(formatProvider) ?? defaultValue;

        [Pure]
        public bool GetBooleanOrDefault(string key, bool defaultValue = default, IFormatProvider? formatProvider = null)
            => kvObject.GetValueOrDefault(key)?.ToBoolean(formatProvider) ?? defaultValue;

        [Pure]
        public byte GetByteOrDefault(string key, byte defaultValue = default, IFormatProvider? formatProvider = null)
            => kvObject.GetValueOrDefault(key)?.ToByte(formatProvider) ?? defaultValue;

        [Pure]
        public decimal GetDecimalOrDefault(string key, decimal defaultValue = default, IFormatProvider? formatProvider = null)
            => kvObject.GetValueOrDefault(key)?.ToDecimal(formatProvider) ?? defaultValue;

        [Pure]
        public double GetDoubleOrDefault(string key, double defaultValue = default, IFormatProvider? formatProvider = null)
            => kvObject.GetValueOrDefault(key)?.ToDouble(formatProvider) ?? defaultValue;

        [Pure]
        public short GetInt16OrDefault(string key, short defaultValue = default, IFormatProvider? formatProvider = null)
            => kvObject.GetValueOrDefault(key)?.ToInt16(formatProvider) ?? defaultValue;

        [Pure]
        public int GetInt32OrDefault(string key, int defaultValue = default, IFormatProvider? formatProvider = null)
            => kvObject.GetValueOrDefault(key)?.ToInt32(formatProvider) ?? defaultValue;

        [Pure]
        public long? GetInt64OrDefault(string key, long? defaultValue = null, IFormatProvider? formatProvider = null)
            => kvObject.GetValueOrDefault(key)?.ToInt64(formatProvider) ?? defaultValue;

        [Pure]
        public sbyte GetSByteOrDefault(string key, sbyte defaultValue = default, IFormatProvider? formatProvider = null)
            => kvObject.GetValueOrDefault(key)?.ToSByte(formatProvider) ?? defaultValue;

        [Pure]
        public float GetSingleOrDefault(string key, float defaultValue = default, IFormatProvider? formatProvider = null)
            => kvObject.GetValueOrDefault(key)?.ToSingle(formatProvider) ?? defaultValue;

        [Pure]
        [return: NotNullIfNotNull(nameof(defaultValue))]
        public object? GetTypeOrDefault(string key, Type conversionType, object? defaultValue = null,  IFormatProvider? provider = null)
            => kvObject.GetValueOrDefault(key)?.ToType(conversionType, provider) ?? defaultValue;

        [Pure]
        public ushort GetUInt16OrDefault(string key, ushort defaultValue = default, IFormatProvider? formatProvider = null)
            => kvObject.GetValueOrDefault(key)?.ToUInt16(formatProvider) ?? defaultValue;

        [Pure]
        public uint GetUInt32OrDefault(string key, uint defaultValue = default, IFormatProvider? formatProvider = null)
            => kvObject.GetValueOrDefault(key)?.ToUInt32(formatProvider) ?? defaultValue;

        [Pure]
        public ulong GetUInt64OrDefault(string key, ulong defaultValue = default, IFormatProvider? formatProvider = null)
            => kvObject.GetValueOrDefault(key)?.ToUInt64(formatProvider) ?? defaultValue;
    }
}
