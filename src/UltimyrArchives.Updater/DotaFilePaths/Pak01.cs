namespace UltimyrArchives.Updater.DotaFilePaths;

internal static class Pak01
{
    internal const string FilePath = "game/dota/pak01_dir.vpk";

    internal const string PatchNotes = "patchnotes/patchnotes.vdpn_c";

    internal const string NpcAbilities = "scripts/npc/npc_abilities.txt";
    internal const string NpcHeroes = "scripts/npc/npc_heroes.txt";
    internal const string Items = "scripts/npc/items.txt";
    internal const string NeutralItems = "scripts/npc/neutral_items.txt";
    internal const string NpcUnits = "scripts/npc/npc_units.txt";
    internal const string NpcAbilityIds = "scripts/npc/npc_ability_ids.txt";

    internal static class Localisation
    {
        private const string LocalisationPatchNotesFormat = "resource/localization/patchnotes/patchnotes_{0}.txt";
        private const string LocalisationAbilitiesFormat = "resource/localization/abilities_{0}.txt";
        private const string LocalisationDotaFormat = "resource/localization/dota_{0}.txt";
        private const string LocalisationHeroLoreFormat = "resource/localization/hero_lore_{0}.txt";

        public static string GetPatchNotes(string language) => string.Format(LocalisationPatchNotesFormat, language);
        public static string GetAbilities(string language) => string.Format(LocalisationAbilitiesFormat, language);
        public static string GetDota(string language) => string.Format(LocalisationDotaFormat, language);
        public static string GetHeroLore(string language) => string.Format(LocalisationHeroLoreFormat, language);
    }
}
