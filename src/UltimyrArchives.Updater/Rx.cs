using System.Text.RegularExpressions;

namespace UltimyrArchives.Updater;

/// <summary>
/// Collection of Regular Expressions used throughout.
/// </summary>
internal static partial class Rx
{
    // Using public properties pointing to the generated methods for now,
    // waiting on .NET 9 to use generated properties and contain refactoring...
    // TODO waiting on Rider/ReSharper support for partial properties...


    [GeneratedRegex(@"Ability(?<index>\d+)", RegexOptions.ExplicitCapture)]
    private static partial Regex _AbilityKey();
    public static Regex AbilityKey { get => _AbilityKey(); }

    [GeneratedRegex(@"([\w]+_empty\d*)|([\w]+_hidden\d*)")]
    private static partial Regex _HiddenOrEmpty();
    public static Regex AbilityHiddenOrEmpty { get => _HiddenOrEmpty(); }
    
    [GeneratedRegex(@"special_bonus_\w+")]
    private static partial Regex _SpecialBonus();
    public static Regex SpecialBonus => _SpecialBonus();
    
    
    [GeneratedRegex(@"<[/]?\s*b\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex _HtmlBold();
    public static Regex HtmlBold => _HtmlBold();

    [GeneratedRegex(@"<[/]?\s*i\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex _HtmlItalics();
    public static Regex HtmlItalics => _HtmlItalics();

    [GeneratedRegex(@"<[/]?\s*[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex _AnyHtmlTag();
    public static Regex HtmlAny => _AnyHtmlTag();
}
