namespace Magus.Data.Models.Stratz.Types;

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
