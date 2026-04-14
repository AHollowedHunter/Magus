using Magus.Common.Dota;

namespace UltimyrArchives.Updater.Utils;

internal sealed class Localizations(Dictionary<string, Dictionary<string, string>> values)
{
    /// <summary>
    /// Attempts to get a localized string via its key.
    /// If the key is not present for the given language, it attempts to
    /// retrieve it from the default language instead (if different).
    /// </summary>
    /// <returns>string if found, null if not</returns>
    public string? GetOrDefault(string key, string language = LanguageMap.DefaultLanguage)
    {
        if (values[language].TryGetValue(key, out var value))
            return value;

        if (language is not LanguageMap.DefaultLanguage)
            values[LanguageMap.DefaultLanguage].TryGetValue(key, out value);
        return value;
    }

    public bool HasLocalization(string key, string language = LanguageMap.DefaultLanguage)
        => values[language].ContainsKey(key);
}
