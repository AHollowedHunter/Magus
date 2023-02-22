namespace Magus.DataBuilder
{
    public static class Dota2GameFiles
    {
        public static readonly string BasePath      = "https://raw.githubusercontent.com/SteamDatabase/GameTracking-Dota2/master/";
        public static readonly string PatchNotes   = BasePath + "game/dota/pak01_dir/patchnotes/patchnotes.vdpn";
        public static readonly string NpcAbilities = BasePath + "game/dota/pak01_dir/scripts/npc/npc_abilities.txt";
        public static readonly string NpcHeroes    = BasePath + "game/dota/pak01_dir/scripts/npc/npc_heroes.txt";
        public static readonly string Items        = BasePath + "game/dota/pak01_dir/scripts/npc/items.txt";
        public static readonly string NeutralItems = BasePath + "game/dota/pak01_dir/scripts/npc/neutral_items.txt";
        public static readonly string NpcUnits     = BasePath + "game/dota/pak01_dir/scripts/npc/npc_units.txt";

        public static class Localization
        {
            private static readonly string patchNotesBase = BasePath + "game/dota/pak01_dir/resource/localization/patchnotes/patchnotes_{0}.txt";
            private static readonly string abilitiesBase  = BasePath + "game/dota/pak01_dir/resource/localization/abilities_{0}.txt";
            private static readonly string dotaBase       = BasePath + "game/dota/pak01_dir/resource/localization/dota_{0}.txt";
            private static readonly string heroLoreBase   = BasePath + "game/dota/pak01_dir/resource/localization/hero_lore_{0}.txt";

            public static string GetPatchNotes(string language) => string.Format(patchNotesBase, language);
            public static string GetAbilities(string language)  => string.Format(abilitiesBase, language);
            public static string GetDota(string language)       => string.Format(dotaBase, language);
            public static string GetHeroLore(string language)   => string.Format(heroLoreBase, language);
        }
    }
}
