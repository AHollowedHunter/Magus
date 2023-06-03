﻿namespace Magus.Data.Models.Stratz.Results
{
    public record QueryRecentResult
    {
        public PlayerType Player { get; init; }

        public record PlayerType
        {
            public IEnumerable<MatchGroupBySteamIdType> MatchGroupBySteamId { get; init; }
            public IEnumerable<MatchGroupByHeroType> MatchGroupByHero { get; init; }

            public SimpleSummaryType SimpleSummary { get; init; }

            public SteamAccountType SteamAccount { get; init; }

            public IList<MatchType> Matches { get; init; }

            public record MatchGroupBySteamIdType : MatchGroupByType { }

            public record MatchGroupByHeroType : MatchGroupByType
            {
                public long HeroId { get; init; }
            }
            public record SimpleSummaryType
            {
                public int MatchCount { get; init; }
                public long LastUpdateDateTime { get; init; }
                public IList<SummaryHeroesType> Heroes { get; init; }

                public record SummaryHeroesType
                {
                    public int HeroId { get; init; }
                    public int WinCount { get; init; }
                    public int LossCount { get; init; }
                }
            }
            public record SteamAccountType
            {
                public string Name { get; init; }
                public string Avatar { get; init; }
            }

            public record MatchType
            {
                public long Id { get; init; }
                public int DurationSeconds { get; init; }
                public long EndDateTime { get; init; }
                public MatchAnalysisOutcome AnalysisOutcome { get; init; }
                public IEnumerable<MatchPlayerType> Players { get; init; }

                public enum MatchAnalysisOutcome
                {
                    NONE,
                    STOMPED,
                    COMEBACK,
                    CLOSE_GAME
                }

                public record MatchPlayerType
                {
                    public bool IsVictory { get; init; }
                    public int HeroId { get; init; }
                    public short Kills { get; init; }
                    public short Deaths { get; init; }
                    public short Assists { get; init; }
                    public int Networth { get; init; }
                    public short ExperiencePerMinute { get; init; }
                    public short GoldPerMinute { get; init; }
                    public MatchPlayerAward Award { get; init; }

                    public enum MatchPlayerAward
                    {
                        NONE,
                        MVP,
                        TOP_CORE,
                        TOP_SUPPORT,
                    }
                }
            }
        }

    }


    public abstract record MatchGroupByType
    {
        public int MatchCount { get; init; }
        public int WinCount { get; init; }
        public int AvgImp { get; init; }
        public int AvgGoldPerMinute { get; init; }
        public int AvgExperiencePerMinute { get; init; }
        public float AvgKills { get; init; }
        public float AvgDeaths { get; init; }
        public float AvgAssists { get; init; }
        public float AvgKDA { get; init; }
    }
}