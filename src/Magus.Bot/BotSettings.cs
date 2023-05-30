namespace Magus.Bot
{
    public class BotSettings
    {
        public string BotToken { get; set; } = string.Empty;

        public ulong DevGuild { get; set; }

        public string BotInvite { get; set; } = string.Empty;
        public string BotServer { get; set; } = string.Empty;
        public string BotPrivacyPolicy { get; set; } = string.Empty;
        public string BotTermsOfService { get; set; } = string.Empty;

        public ulong[] ManagementGuilds { get; set; } = Array.Empty<ulong>();

        public string StratzToken { get; set; }

        public StatusConfig Status { get; set; }
        public SteamConfig Steam { get; set; }
        public AnnouncementConfig Announcements { get; set; }

        public class StatusConfig
        {
            public string Title { get; set; } = string.Empty;
            public byte Type { get; set; } = 0;
        }

        public class SteamConfig
        {
            public string SteamKey { get; set; } = string.Empty;
        }

        public class AnnouncementConfig
        {
            public ulong DotaSource { get; set; }
            public ulong MagusSource { get; set; }
        }
    }
}
