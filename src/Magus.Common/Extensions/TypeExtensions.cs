namespace Magus.Common.Extensions;

public static class TypeExtensions
{
    public static bool IsNumeric(this Type type)
        => Type.GetTypeCode(type) is >= TypeCode.SByte and <= TypeCode.Decimal;
}
