using Magus.Common.Dota.ModelsV2;
using UltimyrArchives.Updater.Extensions;
using UltimyrArchives.Updater.Utils;

namespace UltimyrArchives.Updater.Converters;

public sealed class PatchNoteConverter : KVObjectConverter
{
    public PatchNote Convert(KVObject kvPatch) => new()
    {
        // Only include the patch number i.e. 'patch 7.37' => '7.37'
        PatchNumber       = kvPatch.GetRequiredString("patch_name")[6..],
        Timestamp         = PatchUtils.GetPatchTimestamp(kvPatch),
        Website           = kvPatch["website"]?.ToString(CultureInfo.InvariantCulture),
        GenericNotes      = ConvertList(kvPatch["generic"], ConvertNote),
        HeroesNotes       = ConvertList(kvPatch["heroes"], ConvertHero),
        ItemNotes         = ConvertList(kvPatch["items"], ConvertEntity),
        NeutralItemNotes  = ConvertList(kvPatch["items_neutral"], ConvertEntity),
        NeutralCreepNotes = ConvertList(kvPatch["neutral_creeps"], ConvertEntity)
    };

    private static KVObject[] GetOnlyNotes(KVObject obj)
        => obj.Children.Where(x => x.Name.Equals("note", StringComparison.InvariantCultureIgnoreCase)).ToArray();

    private static Note ConvertNote(KVObject obj) => new(
        obj["indent"]?.ToInt32(CultureInfo.InvariantCulture) ?? 0,
        obj["note"]?.ToString(CultureInfo.InvariantCulture),
        obj["info"]?.ToString(CultureInfo.InvariantCulture)
    );

    private static EntityNote ConvertEntity(KVObject obj) => new(
        obj.Name,
        obj["title"]?.ToString(CultureInfo.InvariantCulture),
        ConvertList(GetOnlyNotes(obj), ConvertNote)
    );

    private static HeroNote ConvertHero(KVObject obj)
    {
        // Assuming all ability are keyed with hero name after 'npc_dota_hero_' i.e. npc_dota_hero_alchemist => alchemist_chemical_rage
        var abilities = obj.Where(x => x.Name.StartsWith(obj.Name[14..], StringComparison.InvariantCultureIgnoreCase)).Select(ConvertEntity).ToArray();

        // Assuming all facets follow the 'hero_facet_N' rule i.e. hero_facet_1, hero_facet_2
        var facets = obj.Where(x => x.Name.StartsWith("hero_facet_", StringComparison.InvariantCultureIgnoreCase)).Select(ConvertEntity).ToArray();

        // So far only used in initial patch 7.36, used to separate innate from 'abilities'
        var innate = obj.SingleOrDefault(x => x.Name == "hero_innate") is { } heroInnate ? ConvertEntity(heroInnate) : null;

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
