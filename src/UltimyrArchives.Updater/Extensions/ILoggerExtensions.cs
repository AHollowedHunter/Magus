using Magus.Data.Enums;
using Microsoft.Extensions.Logging;

namespace UltimyrArchives.Updater.Extensions;

public static partial class ILoggerExtensions
{
    [LoggerMessage(LogLevel.Information, "No children for {entityType} '{entityName}'")]
    public static partial void EntityNoChildren(this ILogger logger, string entityName, string entityType);
    
    [LoggerMessage(LogLevel.Warning, "Duplicate {entityType} name '{entityName}'")]
    public static partial void EntityDuplicate(this ILogger logger, string entityName, string entityType);

    [LoggerMessage(LogLevel.Warning, "Failed parsing for {entityType} '{entityName}'")]
    public static partial void EntityParsingError(this ILogger logger, string entityName, string entityType, Exception? ex = null);
}
