using Ganss.Xss;
using System.Text.RegularExpressions;

namespace Magus.Common.Utilities
{
    public static class DiscordMessageFormatter
    {

        private static readonly Regex _breakRegex   = new(@"(?i)<[/]?\s*br\s*>");
        private static readonly Regex _boldRegex    = new(@"(?i)<[/]?\s*b\s*>");
        private static readonly Regex _italicsRegex = new(@"(?i)<[/]?\s*i\s*>");

        private static readonly HtmlSanitizer _sanitizer;
        static DiscordMessageFormatter()
        {
            var sanitizerOptions = new HtmlSanitizerOptions
            {
                AllowedTags = AllowedTags,
                AllowedAttributes = AllowedAttributes,
            };
            _sanitizer = new HtmlSanitizer(sanitizerOptions);
        }

        public static string HtmlToDiscordMarkdown(string htmlSource)
        {
            var sanitizedSource = _sanitizer.Sanitize(htmlSource);

            sanitizedSource = _breakRegex.Replace(sanitizedSource, "\n");
            sanitizedSource = _boldRegex.Replace(sanitizedSource, "**");
            sanitizedSource = _italicsRegex.Replace(sanitizedSource, "*");

            return sanitizedSource;
        }

        static ISet<string> AllowedTags { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "b", "br", "i", "strong"
        };

        static ISet<string> AllowedAttributes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "href"
        };
    }
}
