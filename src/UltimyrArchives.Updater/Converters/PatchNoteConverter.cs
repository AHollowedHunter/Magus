using Magus.Common.Dota.ModelsV2;
using UltimyrArchives.Updater.Utils;

namespace UltimyrArchives.Updater.Converters;

public sealed class PatchNoteConverter : KVObjectConverter
{
    public PatchNoteManifest Convert(KVObject kvPatch) => new()
    {
        PatchNumber       = PatchUtils.GetPatchNumber(kvPatch),
        Timestamp         = PatchUtils.GetPatchTimestamp(kvPatch),
        Website           = kvPatch["website"]?.ToString(CultureInfo.InvariantCulture),
        GenericNotes      = ConvertList(kvPatch["generic"], ConvertNoteGroup),
        HeroesNotes       = ConvertList(kvPatch["heroes"], ConvertHero),
        ItemNotes         = ConvertList(kvPatch["items"], ConvertNoteGroup),
        NeutralItemNotes  = ConvertList(kvPatch["items_neutral"], ConvertNoteGroup),
        NeutralCreepNotes = ConvertList(kvPatch["neutral_creeps"], ConvertNoteGroup)
    };

    private static KVObject[] GetOnlyNotes(KVObject obj)
        => obj.Children.Where(x => x.Name.Equals("note", StringComparison.InvariantCultureIgnoreCase)).ToArray();

    private static Note ConvertNote(KVObject obj) => new(
        obj["indent"]?.ToInt32(CultureInfo.InvariantCulture) ?? 0,
        TrimKey(obj["note"]?.ToString(CultureInfo.InvariantCulture)),
        TrimKey(obj["info"]?.ToString(CultureInfo.InvariantCulture)),
        obj["scepter"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false,
        obj["shard"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false
    );

    private static NoteGroup ConvertNoteGroup(KVObject obj) => new(
        obj.Name,
        TrimKey(obj["title"]?.ToString(CultureInfo.InvariantCulture)),
        ConvertList(GetOnlyNotes(obj), ConvertNote),
        obj["is_general"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false
    );

    /// <summary>
    /// Remove the beginning '#' from each key, localisation does not include it.
    /// </summary>
    private static string? TrimKey(string? key) => key?.TrimStart('#');

    private static HeroNote ConvertHero(KVObject obj)
    {
        // Assuming all ability are keyed with hero name after 'npc_dota_hero_' i.e. npc_dota_hero_alchemist => alchemist_chemical_rage
        var abilities = obj.Where(x => x.Name.StartsWith(obj.Name[14..], StringComparison.InvariantCultureIgnoreCase)).Select(ConvertNoteGroup).ToArray();

        // Assuming all facets follow the 'hero_facet_N' rule i.e. hero_facet_1, hero_facet_2
        var facets = obj.Where(x => x.Name.StartsWith("hero_facet_", StringComparison.InvariantCultureIgnoreCase)).Select(ConvertNoteGroup).ToArray();

        // So far only used in initial patch 7.36, used to separate innate from 'abilities'
        var innate = obj.SingleOrDefault(x => x.Name == "hero_innate") is { } heroInnate ? ConvertNoteGroup(heroInnate) : null;

        return new HeroNote(
            obj.Name,
            ConvertList(obj["default"], ConvertNote),
            abilities,
            facets,
            innate,
            ConvertList(obj["talent"], ConvertNote)
        );
    }
}
