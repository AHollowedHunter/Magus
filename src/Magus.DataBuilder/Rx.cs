using System.Text.RegularExpressions;

namespace Magus.DataBuilder;

/// <summary>
/// Collection of Regular Expressions used throughout.
/// </summary>
internal static partial class Rx
{
    // Using public properties pointing to the generated methods for now,
    // waiting on .NET 9 to use generated properties and contain refactoring...

    public static Regex AbilityUpgrade => _AbilityUpgrade();
    [GeneratedRegex(@"(?i)(shard|scepter)_\w+|\w+(shard|scepter)")]
    private static partial Regex _AbilityUpgrade();

    public static Regex BonusValues => _BonusValues();
    [GeneratedRegex(@"(?i)LinkedSpecialBonus|ad_linked_abilities|special_bonus_\w+")]
    private static partial Regex _BonusValues();

    public static Regex NonValueName => _NonValueName();
    [GeneratedRegex(@"(?i)special_bonus_\w+|var_type|ad_linked_abilities|LinkedSpecialBonus|RequiresScepter|RequiresShard|\w+[^_]Tooltip|RequiresFacet")]
    private static partial Regex _NonValueName();

    public static Regex NameGender => _NameGender();
    [GeneratedRegex(@"#\|(\p{L}+)\|#")]
    private static partial Regex _NameGender();

    public static Regex TalentName => _TalentName();
    [GeneratedRegex(@"special_bonus_\w+")]
    private static partial Regex _TalentName();

    public static Regex ValueKey => _ValueKey();
    [GeneratedRegex(@"(?<=[+-]?{s:)\w+(?=})")]
    private static partial Regex _ValueKey();

    public static Regex BonusValueKey => _BonusValueKey();
    [GeneratedRegex(@"(?<=[+-]?{s:bonus_)\w+(?=})")]
    private static partial Regex _BonusValueKey();

    public static Regex DoubleSymbols => _DoubleSymbols();
    [GeneratedRegex(@"[%+-](?=[%+-][^%+-])")]
    private static partial Regex _DoubleSymbols();

    public static Regex DigitOnly => _DigitOnly();
    [GeneratedRegex(@"\d+")]
    private static partial Regex _DigitOnly();

    public static Regex OtherDisplayValues => _OtherDisplayValues();
    [GeneratedRegex(@"(?i)\w+(Note\d*|Lore|Description|shard|scepter|abilitydraft_note)")]
    private static partial Regex _OtherDisplayValues();

    public static Regex EscapedPercentage => _EscapedPercentage();
    [GeneratedRegex(@"%%(?=[^%]?)")]
    private static partial Regex _EscapedPercentage();

    public static Regex AbilityValueKey => _AbilityValueKey();
    [GeneratedRegex(@"(?<=%)\w+(?=%)")]
    private static partial Regex _AbilityValueKey();

    public static Regex AbilityBonusValueKey => _AbilityBonusValueKey();
    [GeneratedRegex(@"%\w+%")]
    private static partial Regex _AbilityBonusValueKey();

    public static Regex AbilityVariable => _AbilityVariable();
    [GeneratedRegex(@"\$\w+")]
    private static partial Regex _AbilityVariable();

    public static Regex HtmlBold => _HtmlBold();
    [GeneratedRegex(@"(?i)<[/]?\s*b\s*>")]
    private static partial Regex _HtmlBold();

    public static Regex HtmlItalics => _HtmlItalics();
    [GeneratedRegex(@"(?i)<[/]?\s*i\s*>")]
    private static partial Regex _HtmlItalics();

    public static Regex HtmlAny => _AnyHtmlTag();
    [GeneratedRegex(@"(?i)<[/]?\s*[^>]*>")]
    private static partial Regex _AnyHtmlTag();

    public static Regex PlaceholderSign => _PlaceholderSign();
    [GeneratedRegex(@"[+-](?=\w+)")]
    private static partial Regex _PlaceholderSign();

    public static Regex AbilityKey => _AbilityKey();
    [GeneratedRegex(@"Ability\d+")]
    private static partial Regex _AbilityKey();

    public static Regex AbilityHiddenOrEmpty => _HiddenOrEmpty();
    [GeneratedRegex(@"([\w]+_empty\d*)|([\w]+_hidden\d*)")]
    private static partial Regex _HiddenOrEmpty();

    // Matches each spell, assuming each spell description ends on a line break.
    public static Regex ItemSpells => _ItemSpells();
    [GeneratedRegex(@"<h1>.*?(?=<h1>|\Z|$)", RegexOptions.Singleline)]
    private static partial Regex _ItemSpells();

    public static Regex ItemSpellName => _ItemSpellName();
    [GeneratedRegex(@"<h1>(.+?)</h1>")]
    private static partial Regex _ItemSpellName();

    public static Regex ValuePlaceholder => _ValuePlaceholder();
    [GeneratedRegex(@"%\w+[%]{1,3}")]
    private static partial Regex _ValuePlaceholder();
}
