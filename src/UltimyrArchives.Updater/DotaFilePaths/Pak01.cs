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

    internal static string GetHeroAbilities(string internalName) => $"scripts/npc/heroes/{internalName}.txt";

    internal static class Localization
    {
        private const string LocalizationPatchNotesFormat = "resource/localization/patchnotes/patchnotes_{0}.txt";
        private const string LocalizationAbilitiesFormat = "resource/localization/abilities_{0}.txt";
        private const string LocalizationDotaFormat = "resource/localization/dota_{0}.txt";
        private const string LocalizationHeroLoreFormat = "resource/localization/hero_lore_{0}.txt";

        public static string GetPatchNotes(string language) => string.Format(LocalizationPatchNotesFormat, language);
        public static string GetAbilities(string language) => string.Format(LocalizationAbilitiesFormat, language);
        public static string GetDota(string language) => string.Format(LocalizationDotaFormat, language);
        public static string GetHeroLore(string language) => string.Format(LocalizationHeroLoreFormat, language);
    }
}
