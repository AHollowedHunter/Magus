using Coravel.Scheduling.Schedule.Interfaces;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Options;
using STRATZ;

namespace Magus.Bot.Services
{
    public sealed class StratzService
    {
        private readonly ILogger<AnnouncementService> _logger;
        private readonly IScheduler _scheduler;
        private readonly BotSettings _botSettings;
        private readonly HttpClient _httpClient;

        private readonly GraphQLHttpClient _stratz;

        const string StratzApiUrl = "https://api.stratz.com/graphql";

        public StratzService(ILogger<AnnouncementService> logger, IScheduler scheduler, IOptions<BotSettings> botSettings, IHttpClientFactory httpClientFactory)
        {
            _logger      = logger;
            _scheduler   = scheduler;
            _botSettings = botSettings.Value;
            _httpClient  = httpClientFactory.CreateClient();
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

        public async Task<PlayerType> GetPlayerSummary(long steamId)
        {
            var query = new DotaQueryQueryBuilder()
                .WithPlayer(new PlayerTypeQueryBuilder()
                    .WithSteamAccount(new SteamAccountTypeQueryBuilder()
                            .WithName()
                            .WithAvatar()
                            .WithProfileUri()
                            .WithSeasonRank())
                    .WithSimpleSummary(new PlayerCardHoverTypeQueryBuilder()
                        .WithHeroes(new PlayerCardHoverHeroTypeQueryBuilder()
                            .WithAllScalarFields())
                        .WithLastUpdateDateTime())
                    .WithMatchesGroupBy(new MatchGroupByTypeQueryBuilder()
                        .WithMatchGroupBySteamAccountIdTypeFragment(new MatchGroupBySteamAccountIdTypeQueryBuilder()
                            .WithWinCount()
                            .WithMatchCount()
                            .WithAllScalarFields())
                        , new PlayerMatchesGroupByRequestType()
                        {
                            Take = 25,
                            GameModeIds = new List<object>() {1, 22},
                            PlayerList = FindMatchPlayerList.Single,
                            GroupBy = FindMatchPlayerGroupBy.SteamAccountId,
                        })
                    .WithMatches(new MatchTypeQueryBuilder()
                        .WithId()
                        .WithDurationSeconds()
                        .WithEndDateTime()
                        .WithGameMode()
                        .WithAnalysisOutcome()
                        .WithPlayers(new MatchPlayerTypeQueryBuilder()
                            .WithIsVictory()
                            .WithIsRadiant()
                            .WithAward()
                            .WithHeroId()
                            .WithKills()
                            .WithAssists()
                            .WithDeaths()
                        , steamId)
                    , new PlayerMatchesRequestType() { Take = 25, OrderBy = FindMatchPlayerOrderBy.Desc })
                , steamId)
                .Build();

            var response = await _stratz.SendQueryAsync(new GraphQL.GraphQLRequest(query), () => new { Player = new PlayerType() });
            return response.Data.Player;
        }
    }
}
