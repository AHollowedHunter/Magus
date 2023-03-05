using Discord;

namespace Magus.Bot.Extensions
{
    public static class CustomIdExtensions
    {
        public static (CustomIdMethod Method, string Key) ParseCustomId(string customId)
        {
            customId = customId.TrimStart(Constants.CustomIdParamDelimiter);
            var split = customId.Split(Constants.CustomIdParamDelimiter);
            Enum.TryParse(split[0], out CustomIdMethod method);
            return (method, split[1]);
        }

        public static ButtonBuilder WithCustomId(this ButtonBuilder builder, string commandName, string? groupPrefix = null, string? subGroupPrefix = null, CustomIdMethod method = CustomIdMethod.GET, string? key = null)
        => builder.WithCustomId(CreateCustomId(commandName, groupPrefix, subGroupPrefix, method, key));

        public static SelectMenuBuilder WithCustomId(this SelectMenuBuilder builder, string commandName, string? groupPrefix = null, string? subGroupPrefix = null, CustomIdMethod method = CustomIdMethod.GET, string? key = null)
        => builder.WithCustomId(CreateCustomId(commandName, groupPrefix, subGroupPrefix, method, key));


        private static string CreateCustomId(string commandName, string? groupPrefix = null, string? subGroupPrefix = null, CustomIdMethod method = CustomIdMethod.GET, string? key = null)
        {
            StringBuilder sb = new(capacity: 16, maxCapacity: 100);
            if (groupPrefix != null)
            {
                sb.Append(groupPrefix);
                sb.Append(Constants.CustomIdGroupDelimiter);
            }
            if (subGroupPrefix != null)
            {
                sb.Append(subGroupPrefix);
                sb.Append(Constants.CustomIdGroupDelimiter);
            }
            sb.Append(commandName);
            sb.Append(Constants.CustomIdParamDelimiter);
            sb.Append(method);
            sb.Append(Constants.CustomIdParamDelimiter);
            if (key != null)
                sb.Append(key);
            return sb.ToString();
        }
    }

    public enum CustomIdMethod
    {
        GET,
        SET,
        DEL,
    }
}
