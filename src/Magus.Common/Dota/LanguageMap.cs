using System.Globalization;

namespace Magus.Common.Dota;

public static class LanguageMap
{
    private static readonly IReadOnlyDictionary<string, CultureInfo> Map;

    static LanguageMap()
    {
        /*
         * This should match the Languages available in Dota with a suitable
         * culture info.
         * Update with additional languages if added.
         */
        Map = new Dictionary<string, CultureInfo>(StringComparer.OrdinalIgnoreCase)
        {
            ["english"]    = CultureInfo.GetCultureInfo("en"),
            ["brazilian"]  = CultureInfo.GetCultureInfo("pr-BR"),
            ["bulgarian"]  = CultureInfo.GetCultureInfo("bg"),
            ["czech"]      = CultureInfo.GetCultureInfo("cs"),
            ["danish"]     = CultureInfo.GetCultureInfo("da"),
            ["dutch"]      = CultureInfo.GetCultureInfo("nl"),
            ["finnish"]    = CultureInfo.GetCultureInfo("fi"),
            ["french"]     = CultureInfo.GetCultureInfo("fr"),
            ["german"]     = CultureInfo.GetCultureInfo("de"),
            ["greek"]      = CultureInfo.GetCultureInfo("el"),
            ["hungarian"]  = CultureInfo.GetCultureInfo("hu"),
            ["italian"]    = CultureInfo.GetCultureInfo("it"),
            ["japanese"]   = CultureInfo.GetCultureInfo("ja"),
            ["koreana"]    = CultureInfo.GetCultureInfo("ko"),
            ["latam"]      = CultureInfo.GetCultureInfo("es-419"),
            ["norwegian"]  = CultureInfo.GetCultureInfo("no"),
            ["polish"]     = CultureInfo.GetCultureInfo("pl"),
            ["portuguese"] = CultureInfo.GetCultureInfo("pt"),
            ["romanian"]   = CultureInfo.GetCultureInfo("ro"),
            ["russian"]    = CultureInfo.GetCultureInfo("ru"),
            ["schinese"]   = CultureInfo.GetCultureInfo("zh-CN"),
            ["spanish"]    = CultureInfo.GetCultureInfo("es-ES"),
            ["swedish"]    = CultureInfo.GetCultureInfo("sv-SE"),
            ["tchinese"]   = CultureInfo.GetCultureInfo("zh-TW"),
            ["thai"]       = CultureInfo.GetCultureInfo("th"),
            ["turkish"]    = CultureInfo.GetCultureInfo("tr"),
            ["ukrainian"]  = CultureInfo.GetCultureInfo("uk"),
            ["vietnamese"] = CultureInfo.GetCultureInfo("vi"),
        };
        Languages = Map.Keys.ToArray();
    }

    /// <summary>
    /// The default language should be english, as that is the primary language in Dota.
    /// </summary>
    public static string DefaultLanguage => "english";

    public static CultureInfo DefaultCulture => Map[DefaultLanguage];

    public static string[] Languages { get; }

    public static CultureInfo GetCulture(string language) => Map.GetValueOrDefault(language, DefaultCulture);
}
