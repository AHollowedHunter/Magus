
namespace Magus.Common.Options
{
    public class LocalisationOptions
    {
        /*
         * This probably needs reworking, the language and IETF code mappings probably should be
         * included as a compiled resource instead as they are unlikely to change, and any updates
         * should be manually tested and added anyway.
         * 
         * Consider switching to handling tags regardless of region, e.g. english too
         * see https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo
         */

        /// <summary>
        /// This MUST be the key that matches DefaultTag... or you will mess things up.
        /// </summary>
        /// <remarks>
        /// Like the rest of this, needs rewriting...
        /// </remarks>
        public string DefaultLanguage => "english";

        /// <summary>
        /// Default IETF language tag with region.
        /// </summary>
        /// <remarks>
        /// defaults to en-US as Valve == US company == Simple English.
        /// 
        /// Previously and elsewhere the default is en-GB, so watch for issues with this before
        /// getting around to refactoring/replacing the localisation methods.
        /// </remarks>
        public string DefaultTag { get; set; } = "en-US";
        /// <summary>
        /// Key is language, string array is matching IETF tags
        /// </summary>
        public Dictionary<string, string[]> SourceLocaleMappings { get; set; } = new Dictionary<string, string[]>();

        public IList<string> Locales
            => SourceLocaleMappings.SelectMany(x => x.Value).ToList();

    }
}
