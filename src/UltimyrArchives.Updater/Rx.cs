using System.Text.RegularExpressions;

namespace UltimyrArchives.Updater;

/// <summary>
/// Collection of Regular Expressions used throughout.
/// </summary>
internal static partial class Rx
{
    [GeneratedRegex(@"Ability(?<index>\d+)", RegexOptions.ExplicitCapture)]
    public static partial Regex AbilityKey { get; }

    [GeneratedRegex(@"([\w]+_empty\d*)|([\w]+_hidden\d*)")]
    public static partial Regex AbilityHiddenOrEmpty { get; }

    [GeneratedRegex(@"special_bonus_\w+")]
    public static partial Regex SpecialBonus { get; }

    [GeneratedRegex(@"<[/]?\s*b\s*/?>", RegexOptions.IgnoreCase)]
    public static partial Regex HtmlBold { get; }

    [GeneratedRegex(@"<[/]?\s*i\s*/?>", RegexOptions.IgnoreCase)]
    public static partial Regex HtmlItalics { get; }

    [GeneratedRegex(@"<[/]?\s*[^>]*>", RegexOptions.IgnoreCase)]
    public static partial Regex HtmlAny { get; }
}
