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

        public class LocalisationConfig
        {
            public string DefaultLanguage { get; set; }
            public Dictionary<string, string[]> SourceLocaleMappings { get; set; }
        }
        
        public class DatabaseConfig
        {
            public string ConnectionString { get; set; }
        }
    }
}
