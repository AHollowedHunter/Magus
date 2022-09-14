using Microsoft.Extensions.Configuration;

namespace Magus.Common
{
    public class Configuration
    {
        public Configuration()
        {

        }

        public Configuration(IConfiguration config)
        {
            config.Bind(this);
        }

        public LocalisationConfig Localisation { get; set; }
        public DatabaseConfig DatabaseService { get; set; }
        public SteamConfig Steam { get; set; }

        public string BotToken { get; set; }

        public string BotInvite { get; set; }
        public string BotServer { get; set; }
        public string BotPrivacyPolicy { get; set; }

        public class LocalisationConfig
        {
            public string DefaultLanguage { get; set; }
            public Dictionary<string, string[]> SourceLocaleMappings { get; set; }

            public IList<string> Locales
                => SourceLocaleMappings.SelectMany(x => x.Value).ToList();
        }
        
        public class DatabaseConfig
        {
            public string ConnectionString { get; set; }
            public string DatabaseName { get; set; }
        }

        public class SteamConfig
        {
            public string SteamKey { get; set; }
        }
    }
}
