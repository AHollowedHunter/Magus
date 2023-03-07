
namespace Magus.Common.Options
{
    public class LocalisationOptions
    {
        public string DefaultLanguage { get; set; } = "english";
        public Dictionary<string, string[]> SourceLocaleMappings { get; set; } = new Dictionary<string, string[]>();

        public IList<string> Locales
            => SourceLocaleMappings.SelectMany(x => x.Value).ToList();

    }
}
