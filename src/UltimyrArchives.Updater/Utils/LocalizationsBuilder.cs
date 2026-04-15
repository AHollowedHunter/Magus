using Magus.Common.Dota;
using Serilog;
using UltimyrArchives.Updater.DotaFilePaths;
using ValueConverter = System.Func<string, string>;

namespace UltimyrArchives.Updater.Utils;

internal sealed class LocalizationsBuilder
{
    private readonly GameFileProvider _gameFileProvider;

    private bool _abilities, _dota, _heroLore, _patchNotes;
    private ValueConverter? _abilityConverter, _dotaConverter, _heroLoreConverter, _patchConverter;


    /// <summary>
    /// Build a LocalisedValues dictionary with one or more different KV files.
    /// </summary>
    public LocalizationsBuilder(GameFileProvider gameFileProvider)
    {
        _gameFileProvider = gameFileProvider;
    }

    public async Task<Localizations> BuildAsync()
    {
        var localizations = new Dictionary<string, Dictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var language in LanguageMap.Languages)
            localizations.Add(language, new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase));

        await Parallel.ForEachAsync(localizations, (kvp, _) => AddLocalizationsAsync(kvp.Key, kvp.Value));

        return new Localizations(localizations);
    }

    private async ValueTask AddLocalizationsAsync(string language, Dictionary<string, string> dictionary)
    {
        if (_abilities)
            await AddValues(dictionary, language, Pak01.Localization.GetAbilities(language), _abilityConverter);

        if (_dota)
            await AddValues(dictionary, language, Pak01.Localization.GetDota(language), _dotaConverter);

        if (_heroLore)
            await AddValues(dictionary, language, Pak01.Localization.GetHeroLore(language), _heroLoreConverter);

        if (_patchNotes)
            await AddValues(dictionary, language, Pak01.Localization.GetPatchNotes(language), _patchConverter);
    }

    public LocalizationsBuilder WithAbilities(ValueConverter? valueConverter = null)
    {
        if (_abilities)
            return this;

        _abilities        = true;
        _abilityConverter = valueConverter;
        return this;
    }

    public LocalizationsBuilder WithDota(ValueConverter? valueConverter = null)
    {
        if (_dota)
            return this;

        _dota          = true;
        _dotaConverter = valueConverter;
        return this;
    }

    public LocalizationsBuilder WithHeroLore(ValueConverter? valueConverter = null)
    {
        if (_heroLore)
            return this;

        _heroLore          = true;
        _heroLoreConverter = valueConverter;
        return this;
    }

    public LocalizationsBuilder WithPatchNotes(ValueConverter? valueConverter = null)
    {
        if (_patchNotes)
            return this;

        _patchNotes     = true;
        _patchConverter = valueConverter;
        return this;
    }

    private async ValueTask AddValues(Dictionary<string, string> dictionary, string language, string path, ValueConverter? valueConverter = null)
    {
        KVDocument doc = await _gameFileProvider.GetPak01KVFileAsync(path, new KVSerializerOptions { HasEscapeSequences = true });
        // Some files keep tokens directly under root, e.g. PatchNotes
        KVObject tokens  = doc.Root.GetValueOrDefault("Tokens") ?? doc;
        var      culture = LanguageMap.GetCulture(language);
        foreach (var tokenPair in tokens)
        {
            var value = tokenPair.Value.ToString(culture);
            if (valueConverter != null)
                value = valueConverter(value);
            if (!dictionary.TryAdd(tokenPair.Key, value))
                Log.Warning("Failed to add value for '{Language}' '{TokenPairKey}' as it already exists.", language, tokenPair.Key);
            // throw new InvalidOperationException($"Failed to add value for ('{language}', '{tokenPair.Key}') as it already exists.");
        }
    }
}
