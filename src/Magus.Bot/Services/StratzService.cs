using Coravel.Scheduling.Schedule.Interfaces;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Magus.Data.Models.Stratz.Results;
using Microsoft.Extensions.Options;
using STRATZ;

namespace Magus.Bot.Services;

public sealed class StratzService
{
    private readonly ILogger<StratzService> _logger;
    private readonly IScheduler _scheduler;
    private readonly BotSettings _botSettings;
    private readonly HttpClient _httpClient;

    private readonly GraphQLHttpClient _stratz;

    const string StratzApiUrl = "https://api.stratz.com/graphql";
    //const string StratzApiUrl = "http://127.0.0.1:8000";

    public StratzService(ILogger<StratzService> logger, IScheduler scheduler, IOptions<BotSettings> botSettings, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _scheduler = scheduler;
        _botSettings = botSettings.Value;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", _botSettings.StratzToken);
        _httpClient.BaseAddress = new Uri(StratzApiUrl);

        _stratz = new GraphQLHttpClient(new(), new SystemTextJsonSerializer(), _httpClient);
    }

    public async Task<PlayerType> GetPlayerHeroStats(long steamId, int heroIds, IList<long>? friendIds = null) =>
        await GetPlayerHeroStats(steamId, new List<int>() { heroIds }, friendIds);

    public async Task<PlayerType> GetPlayerHeroStats(long steamId, IList<int> heroIds, IList<long>? friendIds = null)
    {
        var playerHeroPerformanceMatchesRequest = new PlayerHeroPerformanceMatchesRequestType()
        {
            HeroIds = heroIds.Cast<object>().ToList(),
        };
        if (friendIds != null)
            playerHeroPerformanceMatchesRequest.WithFriendSteamAccountIds = friendIds.Cast<object>().ToList();

        var query = new DotaQueryQueryBuilder()
            .WithPlayer(new PlayerTypeQueryBuilder()
                .WithSteamAccount(new SteamAccountTypeQueryBuilder()
                    .WithName())
                .WithHeroesPerformance(
                    new PlayerHeroesPerformanceTypeQueryBuilder()
                        .WithHero(new HeroTypeQueryBuilder()
                            .WithName()
                            .WithId())
                        .WithAllScalarFields()
                    , playerHeroPerformanceMatchesRequest
                    , take: int.MaxValue)
                , steamId)
            .Build();

        var response = await _stratz.SendQueryAsync(new GraphQL.GraphQLRequest(query), () => new { Player = new PlayerType() });
        return response.Data.Player;
    }

    public async Task<StatsRecentResult> GetRecentStats(long accountId)
    {
        var query =
            @"query ($steamid: Long!)
{
  player(steamAccountId: $steamid) {
    MatchGroupBySteamId: matchesGroupBy( request: {
      take: 25
      gameModeIds: [1,22]
      playerList: SINGLE
      groupBy: STEAM_ACCOUNT_ID
    }) {
      ... on MatchGroupBySteamAccountIdType{ matchCount winCount avgImp avgKills avgDeaths avgAssists avgExperiencePerMinute avgGoldPerMinute avgKDA }
    }
    MatchGroupByHero: matchesGroupBy( request: {
      take: 25
      gameModeIds: [1,22]
      playerList: SINGLE
      groupBy: HERO
    }) {
      ... on MatchGroupByHeroType{ heroId matchCount winCount avgKills avgDeaths avgAssists avgExperiencePerMinute avgGoldPerMinute avgKDA avgImp }
    }
    simpleSummary{
      matchCount
      lastUpdateDateTime
      heroes
      {
        heroId
        winCount
        lossCount
      }
    }
    steamAccount {
      name
      avatar
      isAnonymous
    }
    matches( request: {
      isParsed: true
      gameModeIds: [1,22]
      take: 25
      playerList: SINGLE
    }) {
      id
      analysisOutcome
      durationSeconds
      endDateTime
      players(steamAccountId: $steamid) { isVictory networth level assists kills deaths heroId experiencePerMinute goldPerMinute }
    }
  }
}";

        var response = await _stratz.SendQueryAsync<StatsRecentResult>(new GraphQL.GraphQLRequest(query, variables: new { steamid = accountId}));
        return response.Data;
    }

    public async Task<AccountCheckResult> GetAccountInfo(long accountId)
    {
        var query = @"
query ($steamid: Long!)
{
    player(steamAccountId: $steamid) {
        steamAccount {
            name
            avatar
            profileUri
            isAnonymous
        }
    }
}";

        var response = await _stratz.SendQueryAsync<AccountCheckResult>(new GraphQL.GraphQLRequest(query, variables: new { steamid = accountId}));
        return response.Data;
    }

    public async Task<LeagueType> GetLeagueInfo(int leagueId)
    {
        var query = $@"
query ($leagueId: Int!) {{
  league(id: $leagueId) {{
    id
    displayName
    tournamentUrl
    basePrizePool
    prizePool
    tables {{
      tableTeams {{
        teamId
        team {{
          name
          id
        }}
      }}
    }}
    liveMatches {{
      matchId
      delay
      gameState
      gameTime
      completed
      createdDateTime
      radiantScore
      direScore
      direTeam {{
        name
      }}
      radiantTeam {{
        name
      }}
    }}
    nodeGroups {{
      id
      name
      teamCount
      nodeGroupType
      advancingNodeGroupId
      advancingTeamCount
      nodes {{
        id
        nodeGroupId
        seriesId
        nodeType
        winningNodeId
        losingNodeId
        hasStarted
        isCompleted
        scheduledTime
        actualTime
        teamOneWins
        teamTwoWins
        teamOne {{
          id
          name
          tag
        }}
        teamTwo {{
          id
          name
          tag
        }}
      }}
    }}
  }}
}}

";

        var response = await _stratz.SendQueryAsync(new GraphQL.GraphQLRequest(query, variables: new { leagueId }), () => new { League = new LeagueType() });
        return response.Data.League;
    }
}
