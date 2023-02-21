namespace Magus.Bot
{
    public class BotSettings
    {
        public string BotToken { get; set; } = string.Empty;

        public ulong DevGuild { get; set; }

        public string BotInvite { get; set; } = string.Empty;
        public string BotServer { get; set; } = string.Empty;
        public string BotPrivacyPolicy { get; set; } = string.Empty;


        public StatusConfig Status { get; set; }
        public LocalisationConfig Localisation { get; set; }
        public SteamConfig Steam { get; set; }

        public class StatusConfig
        {
            public string Title { get; set; } = string.Empty;
            public byte Type { get; set; } = 0;
        }

        public class LocalisationConfig
        {
            public string DefaultLanguage { get; set; } = "english";
            public Dictionary<string, string[]> SourceLocaleMappings { get; set; }

            public IList<string> Locales
                => SourceLocaleMappings.SelectMany(x => x.Value).ToList();
        }

        public class SteamConfig
        {
            public string SteamKey { get; set; } = string.Empty;
        }
    }
}
