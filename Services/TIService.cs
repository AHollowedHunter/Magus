using Coravel.Scheduling.Schedule.Interfaces;
using Magus.Data;
using System.Net.Http.Json;
using Magus.Common;
using Coravel;
using Coravel.Scheduling.Schedule;
using Magus.Data.Models.Embeds;
using Magus.Data.Models.OpenDota;
using SteamWebAPI2.Utilities;
using SteamWebAPI2.Interfaces;
using System.Text.Json.Serialization;

namespace Magus.Bot.Services
{
    public sealed class TIService
    {
        private readonly IAsyncDataService _db;
        private readonly IScheduler _scheduler;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TIService> _logger;
        private readonly Configuration _config;

        private static readonly uint TI2022_ID = 14268;
        private static readonly uint arlingtonMajorID = 14417; // Temporary using Arligngton Major, as TI isn't on yet
        private static SteamWebInterfaceFactory webInterfaceFactory;

        internal IDictionary<int, string> HeroNames;

        internal IList<Team> Teams;

        public int PrizePool { get; private set; }

        public IDictionary<string, int> HeroPickCount { get; private set; }
        public IDictionary<string, int> HeroBanCount { get; private set; }
        public IEnumerable<string> IgnoredHeroes { get; private set; }
        public HeroesCount MostPickedHero { get; private set; }
        public HeroesCount LeastPickedHero { get; private set; }
        public HeroesCount MostBannedHero { get; private set; }
        public HeroesCount LeastBannedHero { get; private set; }

        public IEnumerable<Match> LongestMatches { get; private set; }
        public IEnumerable<Match> ShortestMatches { get; private set; }

        public IDictionary<string, (int Kills, int Deaths, int Assists)> HeroTotalKDA { get; private set; }

        public TIService(IAsyncDataService db, IScheduler scheduler, HttpClient httpClient, ILogger<TIService> logger, Configuration config)
        {
            _db = db;
            _scheduler = scheduler;
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
            webInterfaceFactory = new(_config.Steam.SteamKey);
        }

        public async Task Initialise()
        {
            await SetHeroMap(); // Need this to run first before anything else, as the map is used in other scheduled tasks
            ScheduleSetHeroMap();
            await UpdateTeams();

            ScheduleUpdatePrizePool();
            ScheduleUpdateStats();

            _logger.LogInformation("TIService Initalised");
        }

        private void ScheduleUpdatePrizePool() => _scheduler.ScheduleAsync(UpdatePrizePool)
                                                            .EveryThirtyMinutes()
                                                            .RunOnceAtStart();

        private async Task UpdatePrizePool()
        {
            try
            {
                var prizePoolResponse = await _httpClient.GetFromJsonAsync<PrizePoolReponse>($"http://api.steampowered.com/IEconDOTA2_570/GetTournamentPrizePool/v1?key={_config.Steam.SteamKey}&leagueid={TI2022_ID}");
                PrizePool = prizePoolResponse.result.prize_pool;
                _logger.LogDebug("Updated TI Prize Pool");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update TI Prize Pool");
            }
        }

        private void ScheduleUpdateStats() => _scheduler.ScheduleAsync(UpdateStats)
                                                        .EveryFiveMinutes()
                                                        .RunOnceAtStart();

        public async Task UpdateStats()
        {
            try
            {
                //var matches = await _httpClient.GetFromJsonAsync<List<Match>>($"https://api.opendota.com/api/leagues/{TI2022_ID}/matches");
                //var matches = await _httpClient.GetFromJsonAsync<List<Match>>($"https://api.opendota.com/api/leagues/{arlingtonMajorID}/matches");
                var leagueMatchIdQuery = $"SELECT+match_id+FROM+matches+WHERE+leagueid+%3D+{arlingtonMajorID}";
                var matchList = await _httpClient.GetFromJsonAsync<MatchList>($"https://api.opendota.com/api/explorer?sql={leagueMatchIdQuery}");
                // Need to parse match IDs and retrieve + store each. Remove any stored if not in recent check
                var matches = await _db.GetRecords<Match>() as List<Match>;
                var newMatches = new List<Match>();
                foreach (var match in matchList.Rows.Where(x => !matches?.Any(match => match.MatchId == x.MatchId) ?? true).Take(10))
                {
                    _logger.LogDebug("Adding matchid {0}", match.MatchId);
                    newMatches.Add(await _httpClient.GetFromJsonAsync<Match>($"https://api.opendota.com/api/matches/{match.MatchId}"));
                }
                if (newMatches.Count > 0)
                {
                    await _db.InsertRecords(newMatches);
                    matches.AddRange(newMatches);
                }

                HeroPickCount = GetTotalPicksBans(matches);
                HeroBanCount  = GetTotalPicksBans(matches, false);
                IgnoredHeroes = HeroNames.Where(x => HeroPickCount[x.Value] == 0 && HeroBanCount[x.Value] == 0)
                                         .Select(x => x.Value);

                var mostPicked  = HeroPickCount.Where(x => x.Value == HeroPickCount.Values.Max());
                var leastPicked = HeroPickCount.Where(x => x.Value == HeroPickCount.Values.Min() && !IgnoredHeroes.Contains(x.Key));
                var mostBanned  = HeroBanCount.Where(x => x.Value == HeroBanCount.Values.Max());
                var leastBanned = HeroBanCount.Where(x => x.Value == HeroBanCount.Values.Min() && !IgnoredHeroes.Contains(x.Key));

                MostPickedHero  = new HeroesCount(mostPicked.Select(x=> x.Key).ToArray(), mostPicked.First().Value);
                LeastPickedHero = new HeroesCount(leastPicked.Select(x => x.Key).ToArray(), leastPicked.First().Value);
                MostBannedHero  = new HeroesCount(mostBanned.Select(x => x.Key).ToArray(), mostBanned.First().Value);
                LeastBannedHero = new HeroesCount(leastBanned.Select(x => x.Key).ToArray(), leastBanned.First().Value);

                LongestMatches = matches.Where(match => match.Duration == matches.Select(x => x.Duration).Max());
                ShortestMatches = matches.Where(match => match.Duration == matches.Select(x => x.Duration).Min());
                

                var steamInterface    = webInterfaceFactory.CreateSteamWebInterface<DOTA2Match>(_httpClient);
                var matchDetails      = await steamInterface.GetMatchDetailsAsync(6707754788);
                var playerSummaryData = matchDetails.Data;
                var mostDeathCount    = playerSummaryData.Players.Select(x => x.Deaths).Max();
                var mostDeaths        = playerSummaryData.Players.Where(x => x.Deaths == mostDeathCount);
                var mostKillsCount    = playerSummaryData.Players.Select(x => x.Kills).Max();
                var mostKills         = playerSummaryData.Players.Where(x => x.Kills == mostKillsCount);
                var mostDeathPlayer   = string.Empty;

                foreach (var player in mostDeaths)
                    mostDeathPlayer += player.AccountId + " ";
                var mostKillsPlayer = string.Empty;
                foreach (var player in mostKills)
                    mostKillsPlayer += player.AccountId + " ";


                var heroKDA = new Dictionary<string, (int Kills, int Deaths, int Assists)>();
                foreach (var hero in HeroNames)
                {
                    var kills = matches.SelectMany(x => x.Players)
                                   .Where(x => x.HeroId == hero.Key)
                                   .Select(x => x.Kills)
                                   .Sum();
                    var deaths = matches.SelectMany(x => x.Players)
                                   .Where(x => x.HeroId == hero.Key)
                                   .Select(x => x.Deaths)
                                   .Sum();
                    var assists = matches.SelectMany(x => x.Players)
                                   .Where(x => x.HeroId == hero.Key)
                                   .Select(x => x.Assists)
                                   .Sum();
                    heroKDA.Add(hero.Value, (kills, deaths, assists));
                }
                HeroTotalKDA = heroKDA;

                _logger.LogDebug("Updated TI Match Stats");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update TI Match Stats");
            }
        }

        private IDictionary<string, int> GetTotalPicksBans(IList<Match> matches, bool isPick = true)
        {
            var totals = new Dictionary<string, int>();
            foreach (var hero in HeroNames)
            {
                var count = matches.SelectMany(x => x.PicksBans)
                                   .Where(x => x.IsPick == isPick && x.HeroId == hero.Key)
                                   .Count();
                totals.Add(hero.Value, count);
            }
            return totals;
        }

        private void ScheduleSetHeroMap() => _scheduler.ScheduleAsync(SetHeroMap)
                                                       .Daily();

        private async Task SetHeroMap()
        {
            var heroes = new SortedDictionary<int, string>();
            var heroInfos = await _db.GetRecords<HeroInfoEmbed>(locale: "en-GB");
            foreach (var hero in heroInfos)
                heroes.Add(hero.EntityId, hero.Name);
            this.HeroNames = heroes;
        }

        private async Task UpdateTeams()
        {
            var teams = await _httpClient.GetFromJsonAsync<List<Team>>($"https://api.opendota.com/api/leagues/{arlingtonMajorID}/teams");
            if (teams != null)
                Teams = teams;
        }

        public struct PrizePoolReponse
        {
            public Result result { get; set; }
            public struct Result
            {
                public int prize_pool { get; set; }
                public int league_id { get; set; }
                public int status { get; set; }
            }
        }

        public struct MatchList
        {
            [JsonPropertyName("rows")]
            public IList<Match> Rows { get; set; }

            public struct Match
            {
                [JsonPropertyName("match_id")]
                public ulong MatchId { get; set; }
            }
        }

        public struct HeroesCount
        {
            public HeroesCount(string[] names, int count)
            {
                HeroNames = names;
                Count     = count;
            }
            public string[] HeroNames { get; }
            public int Count { get; }
        }

        public struct MatchesLength
        {
            public MatchesLength(Match[] matches, TimeSpan duration)
            {
                Matches = matches;
                Duration = duration;
            }
            public Match[] Matches { get; }
            public TimeSpan Duration { get; }
        }
    }
}
