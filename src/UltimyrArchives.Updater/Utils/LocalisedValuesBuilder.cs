using Magus.Common.Dota;
using System.Collections.Concurrent;
using UltimyrArchives.Updater.DotaFilePaths;

namespace UltimyrArchives.Updater.Utils;

/// <summary>
/// Build a LocalisedValues dictionary with one or more different KV files.
/// </summary>
internal sealed class LocalisedValuesBuilder(GameFileProvider gameFileProvider)
{
    private readonly ConcurrentDictionary<(string Language, string Key), string> _values = [];
    private bool _abilities, _dota, _heroLore, _patchNotes;
    private readonly List<Task> _buildTasks = [];

    public async Task<LocalisedValues> BuildAsync()
    {
        await Task.WhenAll(_buildTasks).ConfigureAwait(false);
        return new LocalisedValues(_values.ToDictionary());
    }

    public LocalisedValuesBuilder WithAbilities(Func<string, string>? valueConverter = null)
    {
        if (_abilities)
            return this;
        _abilities = true;

        _buildTasks.Add(
            Parallel.ForEachAsync(
                LanguageMap.Languages,
                (language, _) => AddTokensAsync(language, Pak01.Localisation.GetAbilities(language), valueConverter)));

        return this;
    }

    public LocalisedValuesBuilder WithDota(Func<string, string>? valueConverter = null)
    {
        if (_dota)
            return this;
        _dota = true;

        _buildTasks.Add(
            Parallel.ForEachAsync(
                LanguageMap.Languages,
                (language, _) => AddTokensAsync(language, Pak01.Localisation.GetDota(language), valueConverter)));

        return this;
    }

    public LocalisedValuesBuilder WithHeroLoreAsync(Func<string, string>? valueConverter = null)
    {
        if (_heroLore)
            return this;
        _heroLore = true;

        _buildTasks.Add(
            Parallel.ForEachAsync(
                LanguageMap.Languages,
                (language, _) => AddTokensAsync(language, Pak01.Localisation.GetHeroLore(language), valueConverter)));

        return this;
    }

    public LocalisedValuesBuilder WithPatchNotes(Func<string, string>? valueConverter = null)
    {
        if (_patchNotes)
            return this;
        _patchNotes = true;

        _buildTasks.Add(
            Parallel.ForEachAsync(
                LanguageMap.Languages,
                (language, _) => AddChildrenAsync(language, Pak01.Localisation.GetPatchNotes(language), valueConverter)));

        return this;
    }

    /// <summary>
    /// Use for localisation files where the key-values are children under the document. e.g. PatchNotes
    /// </summary>
    private async ValueTask AddChildrenAsync(string language, string path, Func<string, string>? valueConverter = null)
    {
        var values = await gameFileProvider.GetPak01KVFileAsync(path, new KVSerializerOptions() { HasEscapeSequences = true });
        AddValues(language, values, valueConverter);
    }

    /// <summary>
    /// Used for standard 'lang' localisation files.
    /// </summary>
    private async ValueTask AddTokensAsync(string language, string path, Func<string, string>? valueConverter = null)
    {
        var values = await gameFileProvider.GetPak01KVFileAsync(path, new KVSerializerOptions() { HasEscapeSequences = true });
        AddValues(language, values.Children.Single(x => x.Name.Equals("Tokens", StringComparison.InvariantCultureIgnoreCase)), valueConverter);
    }

    private void AddValues(string language, KVObject valuesObj, Func<string, string>? valueConverter = null)
    {
        foreach (var valueObj in valuesObj)
        {
            var value = valueObj.Value.ToString(LanguageMap.GetCulture(language));
            if (valueConverter != null)
                value = valueConverter(value);
            if (!_values.TryAdd((language, valueObj.Name), value))
                throw new InvalidOperationException($"Failed to add value for ('{language}', '{valueObj.Name}') as it already exists.");
        }
    }
}
