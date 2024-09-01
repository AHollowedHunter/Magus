using Magus.Common.Dota;

namespace UltimyrArchives.Updater.Utils;

internal sealed class LocalisedValues(Dictionary<(string Language, string Key), string> values)
{
    public string this[string language, string key] => values[(language, key)];

    public string? GetValueOrDefault(string language, string key)
    {
        if (values.TryGetValue((language, key), out var value))
            return value;

        values.TryGetValue((LanguageMap.DefaultLanguage, key), out value);
        return value;
    }
}
