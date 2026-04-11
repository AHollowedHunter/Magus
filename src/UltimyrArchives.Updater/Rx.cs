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

    /// <summary>
    /// We specifically exclude anything with 'facet' due to lingering values
    /// that are just not worth dealing with (was only a handful of facets
    /// before they were removed creating an edge case).
    /// </summary>
    [GeneratedRegex(@"special_bonus_(?!facet)\w+")]
    public static partial Regex SpecialBonus { get; }

    [GeneratedRegex(@"<[/]?\s*b\s*/?>", RegexOptions.IgnoreCase)]
    public static partial Regex HtmlBold { get; }

    [GeneratedRegex(@"<[/]?\s*i\s*/?>", RegexOptions.IgnoreCase)]
    public static partial Regex HtmlItalics { get; }

    [GeneratedRegex(@"<[/]?\s*[^>]*>", RegexOptions.IgnoreCase)]
    public static partial Regex HtmlAny { get; }
}
