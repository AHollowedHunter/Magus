using Ganss.Xss;
using System.Text.RegularExpressions;

namespace Magus.Common.Utilities;

public static partial class DiscordMessageFormatter
{
    private static readonly HtmlSanitizer _sanitizer;
    private static readonly ReverseMarkdown.Converter _markdownConverter;
    static DiscordMessageFormatter()
    {
        var sanitizerOptions = new HtmlSanitizerOptions { AllowedTags = AllowedTags, AllowedAttributes = AllowedAttributes, };
        _sanitizer         = new HtmlSanitizer(sanitizerOptions);
        _markdownConverter = new(new() { CleanupUnnecessarySpaces = false });
    }

    public static string HtmlToDiscordEmbedMarkdown(string htmlSource, bool sanitize = true)
    {
        // TODO tidy/check/improve, probably this whole class...
        var sanitizedSource = sanitize ? _sanitizer.Sanitize(htmlSource) : htmlSource;

        sanitizedSource = RssRegex.Replace(sanitizedSource, ""); // DO this first to prevent inadvertently removing Markdown URLs
        sanitizedSource = _markdownConverter.Convert(sanitizedSource);

        return sanitizedSource;
    }

    private static HashSet<string> AllowedTags { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "b", "br", "div", "i", "li", "p", "ol", "strong", "ul"
    };

    private static HashSet<string> AllowedAttributes { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "href"
    };

    [GeneratedRegex(@"\[[^\]]*\](?:.*)\[/[^\]]*\]")]
    private static partial Regex RssRegex { get; }
}
