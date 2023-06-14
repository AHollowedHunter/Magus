namespace Magus.Data.Models.Stratz.Results
{
    public record AccountCheckResult
    {
        public PlayerType? Player { get; init; }

        public record PlayerType
        {
            public SteamAccountType SteamAccount { get; init; }

            public record SteamAccountType
            {
                public string Name { get; init; }
                public string Avatar { get; init; }
                public string ProfileUri { get; init; }
                public bool IsAnonymous { get; init; }
            }
        }
    }
}
