using System.ComponentModel.DataAnnotations;

namespace Magus.Common.Extensions
{
    public static class EnumExtensions
    {
        public static string ToName(this Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())[0]
                .GetCustomAttributes(false)
                .Select(x => x as DisplayAttribute)
                .FirstOrDefault();
            return displayAttribute?.Name ?? enumValue.ToString();
        }

        public static string? GetDisplayName(this Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())[0]
                .GetCustomAttributes(false)
                .Select(x => x as DisplayAttribute)
                .FirstOrDefault();
            return displayAttribute?.Name;
        }
    }
}
