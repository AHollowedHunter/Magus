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
}
