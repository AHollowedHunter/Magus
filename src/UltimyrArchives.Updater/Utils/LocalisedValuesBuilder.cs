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

    public LocalisedValuesBuilder WithAbilities()
    {
        if (_abilities)
            return this;
        _abilities = true;

        _buildTasks.Add(
            Parallel.ForEachAsync(
                LanguageMap.Languages,
                (language, _) => AddValuesAsync(language, Pak01.Localisation.GetAbilities(language))));

        return this;
    }

    public LocalisedValuesBuilder WithDota()
    {
        if (_dota)
            return this;
        _dota = true;

        _buildTasks.Add(
            Parallel.ForEachAsync(
                LanguageMap.Languages,
                (language, _) => AddValuesAsync(language, Pak01.Localisation.GetDota(language))));

        return this;
    }

    public LocalisedValuesBuilder WithHeroLoreAsync()
    {
        if (_heroLore)
            return this;
        _heroLore = true;

        _buildTasks.Add(
            Parallel.ForEachAsync(
                LanguageMap.Languages,
                (language, _) => AddValuesAsync(language, Pak01.Localisation.GetHeroLore(language))));

        return this;
    }

    public LocalisedValuesBuilder WithPatchNotes()
    {
        if (_patchNotes)
            return this;
        _patchNotes = true;

        _buildTasks.Add(
            Parallel.ForEachAsync(
                LanguageMap.Languages,
                (language, _) => AddValuesAsync(language, Pak01.Localisation.GetPatchNotes(language))));

        return this;
    }

    private async ValueTask AddValuesAsync(string language, string path)
    {
        var values = await gameFileProvider.GetPak01KVFileAsync(path, new KVSerializerOptions() { HasEscapeSequences = true });
        foreach (var value in values)
            _values.TryAdd((language, value.Name), value.Value.ToString(LanguageMap.GetCulture(language)));
    }
}
