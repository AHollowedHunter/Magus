using Coravel.Scheduling.Schedule.Interfaces;
using Magus.Data;
using System.Net.Http.Json;
using Magus.Common;
using Coravel;
using Coravel.Scheduling.Schedule;

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

        public int PrizePool { get; private set; }

        public TIService(IAsyncDataService db, IScheduler scheduler, HttpClient httpClient, ILogger<TIService> logger, Configuration config)
        {
            _db = db;
            _scheduler = scheduler;
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
        }

        public void Initialise()
        {
            ScheduleUpdatePrizePool();
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
                _logger.LogWarning(ex, "Failed to update TI Prize Pool.");
            }
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

    }
}
