using Magus.Common.Dota.ModelsV2;
using System.Diagnostics;
using UltimyrArchives.Updater.Extensions;
using UltimyrArchives.Updater.Utils;

namespace UltimyrArchives.Updater.Converters;

public sealed class PatchNoteConverter : KVObjectConverter
{
    public PatchNoteManifest Convert(KVObject kvPatch) => new()
    {
        PatchNumber       = DotaUtils.GetPatchNumber(kvPatch),
        Timestamp         = DotaUtils.GetPatchTimestamp(kvPatch),
        Website           = kvPatch.GetStringOrDefault("website", formatProvider: CultureInfo.InvariantCulture),
        GenericNotes      = kvPatch["generic"].Select(ConvertNoteGroup).ToArray(),
        HeroesNotes       = kvPatch["heroes"].Select(ConvertHero).ToArray(),
        ItemNotes         = kvPatch["items"].Select(ConvertNoteGroup).ToArray(),
        NeutralItemNotes  = kvPatch["items_neutral"].Select(ConvertNoteGroup).ToArray(),
        NeutralCreepNotes = kvPatch["neutral_creeps"].Select(ConvertNoteGroup).ToArray(),
    };

    private static IEnumerable<KVOPair> GetOnlyNotes(KVObject obj)
        => obj.Children.Where(x => x.Key.Equals("note", StringComparison.InvariantCultureIgnoreCase));

    private static Note ConvertNote(KVOPair obj) => new(
        obj.Value.GetInt32OrDefault("indent", 0, CultureInfo.InvariantCulture),
        TrimKey(obj.Value.GetStringOrDefault("note", formatProvider: CultureInfo.InvariantCulture)),
        TrimKey(obj.Value.GetStringOrDefault("info", formatProvider: CultureInfo.InvariantCulture)),
        obj.Value.GetBooleanOrDefault("scepter", formatProvider: CultureInfo.InvariantCulture),
        obj.Value.GetBooleanOrDefault("shard", formatProvider: CultureInfo.InvariantCulture)
    );

    private static NoteGroup ConvertNoteGroup(KVOPair obj)
    {
        try
        {
            obj.Value.GetValueOrDefault("title")?.ToString(CultureInfo.InvariantCulture);
            GetOnlyNotes(obj.Value).Select(ConvertNote);
            obj.Value.GetValueOrDefault("is_general")?.ToBoolean(CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Debugger.Break();
        }

        return new NoteGroup(
            obj.Key,
            TrimKey(obj.Value.GetValueOrDefault("title")?.ToString(CultureInfo.InvariantCulture)),
            GetOnlyNotes(obj.Value).Select(ConvertNote).ToArray(),
            obj.Value.GetValueOrDefault("is_general")?.ToBoolean(CultureInfo.InvariantCulture) ?? false
        );
    }

    /// <summary>
    /// Remove the beginning '#' from each key, localisation does not include it.
    /// </summary>
    private static string? TrimKey(string? key) => key?.TrimStart('#');

    private static HeroNote ConvertHero(KVOPair obj)
    {
        // Assuming all ability are keyed with hero name after 'npc_dota_hero_' i.e. npc_dota_hero_alchemist => alchemist_chemical_rage
        var abilities = obj.Value.Where(x => x.Key.StartsWith(DotaUtils.GetShortHeroName(obj.Key), StringComparison.InvariantCultureIgnoreCase))
            .Select(ConvertNoteGroup)
            .ToArray();

        // Assuming all facets follow the 'hero_facet_N' rule i.e. hero_facet_1, hero_facet_2
        var facets = obj.Value.Where(x => x.Key.StartsWith("hero_facet_", StringComparison.InvariantCultureIgnoreCase)).Select(ConvertNoteGroup).ToArray();

        // So far only used in initial patch 7.36, used to separate innate from 'abilities'
        var innate = obj.Value.SingleOrDefault(x => x.Key == "hero_innate") is { Value: not null } heroInnate ? ConvertNoteGroup(heroInnate) : null;

        return new HeroNote(
            obj.Key,
            obj.Value.GetValueOrDefault("default", []).Select(ConvertNote).ToArray(),
            abilities,
            facets,
            innate,
            obj.Value.GetValueOrDefault("talent", []).Select(ConvertNote).ToArray()
        );
    }
}
