namespace Magus.DataBuilder;

public static class Dota2GameFiles
{
    public static string BasePath { get; set; }

    public static string NpcFolder => BasePath + "/scripts/npc";
    public static string HeroesFolder => NpcFolder + "/heroes";

    public static string PatchNotes    => BasePath + "/patchnotes/patchnotes.vdpn";
    public static string NpcAbilities  => NpcFolder + "/npc_abilities.txt";
    public static string NpcHeroes     => NpcFolder + "/npc_heroes.txt";
    public static string Items         => NpcFolder + "/items.txt";
    public static string NeutralItems  => NpcFolder + "/neutral_items.txt";
    public static string NpcUnits      => NpcFolder + "/npc_units.txt";
    public static string NpcAbilityIds => NpcFolder + "/npc_ability_ids.txt";

    public static class Localization
    {
        private static string PatchNotesBase => BasePath + "/resource/localization/patchnotes/patchnotes_{0}.txt";
        private static string AbilitiesBase  => BasePath + "/resource/localization/abilities_{0}.txt";
        private static string DotaBase       => BasePath + "/resource/localization/dota_{0}.txt";
        private static string HeroLoreBase   => BasePath + "/resource/localization/hero_lore_{0}.txt";

        public static string GetPatchNotes(string language) => string.Format(PatchNotesBase, language);
        public static string GetAbilities(string language) => string.Format(AbilitiesBase, language);
        public static string GetDota(string language) => string.Format(DotaBase, language);
        public static string GetHeroLore(string language) => string.Format(HeroLoreBase, language);
    }
}
