using Ganss.Xss;
using System.Text.RegularExpressions;

namespace Magus.Common.Utilities
{
    public static class DiscordMessageFormatter
    {
        private static readonly Regex _rssRegex = new(@"\[[^\]]*\](?:.*)\[/[^\]]*\]");

        private static readonly HtmlSanitizer _sanitizer;
        private static readonly ReverseMarkdown.Converter _markdownConverter;
        static DiscordMessageFormatter()
        {
            var sanitizerOptions = new HtmlSanitizerOptions
            {
                AllowedTags = AllowedTags,
                AllowedAttributes = AllowedAttributes,
            };
            _sanitizer = new HtmlSanitizer(sanitizerOptions);
            _markdownConverter = new();
        }

        public static string HtmlToDiscordMarkdown(string htmlSource)
        {
            var sanitizedSource = _sanitizer.Sanitize(htmlSource);

            sanitizedSource = _markdownConverter.Convert(sanitizedSource);
            sanitizedSource = _rssRegex.Replace(sanitizedSource, "");


            return sanitizedSource;
        }

        static ISet<string> AllowedTags { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "b", "br", "div", "i", "li", "ol", "strong", "ul"
        };

        static ISet<string> AllowedAttributes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "href"
        };
    }
}
