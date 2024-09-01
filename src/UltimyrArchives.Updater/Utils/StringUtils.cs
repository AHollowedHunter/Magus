using System.Text.RegularExpressions;

namespace UltimyrArchives.Updater.Utils;

public static partial class StringUtils
{
    public static string CleanSimple(string value)
    {
        value = value.Replace("*", "\\*"); // Escape existing asterisks
        value = value.Replace("<br>", "\n");
        value = value.Replace("&nbsp;", "\u00A0");
        value = Rx.HtmlBold.Replace(value, "**");
        value = Rx.HtmlItalics.Replace(value, "*");
        value = Rx.HtmlAny.Replace(value, "");

        return value;
    }

    public static string CleanPatchNote(string value)
    {
        value = OnlyBreak.Replace(value, "\n");
        value = Table.Replace(value, ""); // For now, remove whole table content.
        value = Highlight.Replace(value, "__"); // Replace highlighted content with underline

        value = CleanSimple(value);

        return value;
    }


    [GeneratedRegex(@"^\s*<br>\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex _OnlyBreak();

    public static Regex OnlyBreak => _OnlyBreak();

    [GeneratedRegex(@"<table>(.|\n)*<\/table>", RegexOptions.IgnoreCase)]
    private static partial Regex _Table();

    public static Regex Table => _Table();


    [GeneratedRegex(@"</?\s*(span|font)[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex _Highlight();

    public static Regex Highlight => _Highlight();
}
