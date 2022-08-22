namespace Magus.DataBuilder
{
    public static class Dota2GameFiles
    {
        public static readonly string BaseUri = "https://raw.githubusercontent.com/SteamDatabase/GameTracking-Dota2/master/";
        public static readonly string PatchNotes = BaseUri + "game/dota/pak01_dir/patchnotes/patchnotes.vdpn";

        public static class Localization
        {
            private static readonly string languageTag = "%LANG%";
            private static readonly string patchNotesBase = BaseUri + $"game/dota/pak01_dir/resource/localization/patchnotes/patchnotes_{languageTag}.txt";

            public static string GetPatchNotes(string language) => patchNotesBase.Replace(languageTag, language);
        }
    }
}
