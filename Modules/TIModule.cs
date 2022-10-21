using Discord;
using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.Services;
using Magus.Common;
using Magus.Data;
using Magus.Data.Models.Embeds;
using Magus.Data.Models.OpenDota;
using Steam.Models;
using System.Net.Http.Json;

namespace Magus.Bot.Modules
{
    [Group("ti", "The International commands")]
    [ModuleRegistration(Location.GLOBAL)]
    public sealed class TIModule : ModuleBase
    {
        private readonly IAsyncDataService _db;
        private readonly Configuration _config;
        private readonly HttpClient _httpClient;
        private readonly TIService _tiService;

        private static readonly uint TI2022_ID = 14268;

        public TIModule(IAsyncDataService db, Configuration config, HttpClient httpClient, TIService tiService)
        {
            _db         = db;
            _config     = config;
            _httpClient = httpClient;
            _tiService  = tiService;
        }

        [SlashCommand("prize-pool", "Get current TI Prizepool")]
        public async Task PrizePool()
        {
            var embed = new EmbedBuilder()
            {
                Title        = "The International 2022 Prize Pool",
                Description  = $"Current Prize Pool stands at:\n\n**${string.Format("{0:n0}", _tiService.PrizePool)}**",
                Timestamp    = DateTimeOffset.UtcNow,
                Color        = Color.Gold,
                ThumbnailUrl = DotaUrls.DotaColourLogo,
            };
            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("live", "Get live games")]
        public async Task LiveGames()
        {
            var embed = new EmbedBuilder()
            {
                Title        = "The International 2022 Live Games",
                Timestamp    = DateTimeOffset.UtcNow,
                Color        = Color.Gold,
                ThumbnailUrl = DotaUrls.DotaColourLogo,
            };

            foreach (var game in _tiService.LiveGames)
            {
                var gameField = new EmbedFieldBuilder();
                gameField.Name = $"{game.RadiantTeam?.TeamName ?? "[UNKNOWN]"} vs {game.DireTeam?.TeamName ?? "[UNKNOWN]"}";

                var value = $"Duration:\u2007**{game.Duration.ToString(@"h\:mm\:ss")}**{Emotes.Spacer}*(Stream Delay:\u2007{game.StreamDelaySeconds.TotalSeconds}s)*\n"
                            + $"Score:\u2007||**{game.Scores.Radiant}\u00A0-\u00A0{game.Scores.Dire}**||{Emotes.Spacer}"
                            + (game.SeriesWins.Radiant > 0 || game.SeriesWins.Dire > 0 ? $"Series Wins:\u2007**||{game.SeriesWins.Radiant}\u00A0-\u00A0{game.SeriesWins.Dire}||**\n" : "\n")
                            + $"Match ID:\u2007{game.MatchId}";

                gameField.Value = value;
                embed.AddField(gameField);
            }
            if (!_tiService.LiveGames.Any())
                embed.Description = "No live games right now.\nCheck the schedule here: https://www.dota2.com/esports/ti11/schedule";

            await RespondAsync(embed: embed.Build());
        }


        //[SlashCommand("match-stats", "Get current TI Match Stats")]
        public async Task MatchStats()
        {
            var longMatches = string.Empty;
            foreach (var match in _tiService.LongestMatches)
                longMatches += $"Longest Game: **{TimeSpan.FromSeconds(match.Duration).ToString("c")} - {_tiService.Teams.Where(x => x.TeamId == match.RadiantTeamId).First().Name}** vs **{_tiService.Teams.Where(x => x.TeamId == match.DireTeamId).First().Name}**\n";
            var shortMatches = string.Empty;
            foreach (var match in _tiService.ShortestMatches)
                shortMatches += $"Shortest Game: **{TimeSpan.FromSeconds(match.Duration).ToString("c")} - {_tiService.Teams.Where(x => x.TeamId == match.RadiantTeamId).First().Name}** vs **{_tiService.Teams.Where(x => x.TeamId == match.DireTeamId).First().Name}**\n";
            var embed = new EmbedBuilder()
            {
                Title        = "The International 2022 Match Stats",
                Description  = $"Most Picked Hero: **{string.Join(" | ", _tiService.MostPickedHero.HeroNames)} - {_tiService.MostPickedHero.Count}**\n"
                               + $"Least Picked Hero: **{string.Join(" | ", _tiService.LeastPickedHero.HeroNames)} - {_tiService.LeastPickedHero.Count}**\n"
                               + $"Most Banned Hero: **{string.Join(" | ", _tiService.MostBannedHero.HeroNames)} - {_tiService.MostBannedHero.Count}**\n"
                               + $"Least Banned Hero: **{string.Join(" | ", _tiService.LeastBannedHero.HeroNames)} - {_tiService.LeastBannedHero.Count}**\n"
                               + $"Ignored Heroes: **{string.Join(" | ", _tiService.IgnoredHeroes)}**\n"
                               + longMatches
                               + shortMatches,
                Timestamp    = DateTimeOffset.UtcNow,
                Color        = Color.Gold,
                ThumbnailUrl = DotaUrls.DotaColourLogo,
            };
            await RespondAsync(embed: embed.Build());
        }

        //[SlashCommand("hero-stats", "Get current TI Match Stats")]
        public async Task HeroStats()
        {
            var mostKillsHero   = _tiService.HeroTotalKDA.First(x => x.Value.Kills == _tiService.HeroTotalKDA.Select(x => x.Value.Kills).Max());
            var mostDeathshero  = _tiService.HeroTotalKDA.First(x => x.Value.Deaths == _tiService.HeroTotalKDA.Select(x => x.Value.Deaths).Max());
            var mostAssistsHero = _tiService.HeroTotalKDA.First(x => x.Value.Assists == _tiService.HeroTotalKDA.Select(x => x.Value.Assists).Max());

            var embed = new EmbedBuilder()
            {
                Title        = "The International 2022 Hero Stats",
                Description  = $"Highest Total Kills: **{mostKillsHero.Value.Kills}** as {mostKillsHero.Key}\n"
                               + $"Highest Total Deaths: **{mostDeathshero.Value.Deaths}** as {mostDeathshero.Key}\n"
                               + $"Highest Total Assists: **{mostAssistsHero.Value.Assists}** as {mostAssistsHero.Key}\n",
                Timestamp    = DateTimeOffset.UtcNow,
                Color        = Color.Gold,
                ThumbnailUrl = DotaUrls.DotaColourLogo,
            };
            await RespondAsync(embed: embed.Build());
        }

        //[SlashCommand("test", "testing")]
        public async Task test()
        {
            await RespondAsync("Processing...");
            //var matches = await _httpClient.GetFromJsonAsync<List<Match>>($"https://api.opendota.com/api/leagues/{TI2022_ID}/matches");
            var matches = await _httpClient.GetFromJsonAsync<List<Match>>($"https://api.opendota.com/api/leagues/14417/matches");
            //var matchesString = await _httpClient.GetStringAsync($"https://api.opendota.com/api/leagues/14417/matches");
            //var matches = JsonSerializer.Deserialize<List<Match>>(matchesString);

            //string fileName = @"U:\workspace\Magus\Data\arlignton_matches.txt";
            //string jsonString = File.ReadAllText(fileName);
            //var matches = JsonSerializer.Deserialize<List<Match>>(jsonString)!;

            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            //var heroPickCounts = matches.SelectMany(x=> x.PicksBans)
            //                            .Where(x => x.IsPick == true)
            //                            .Select(x => x.HeroId)
            //                            .GroupBy(x => x)
            //                            .OrderBy(x => x.Key)
            //                            .Select(x => new { Key = x.Key, Value = x.Count() })
            //                            .ToDictionary(x=>x.Key, x=> x.Value);
            //var heroBanCounts = matches.SelectMany(x=> x.PicksBans)
            //                           .Where(x => x.IsPick == false)
            //                           .Select(x => x.HeroId)
            //                           .GroupBy(x => x)
            //                           .OrderBy(x => x.Key)
            //                           .Select(x => new { Key = x.Key, Value = x.Count() })
            //                           .ToDictionary(x=>x.Key, x=> x.Value);


            //var longestGameList = matches.Where(x => x.Duration == matches.Select(x => x.Duration).Max()).Select(x=> new {Key = x.MatchId, Value = x.Duration}).ToDictionary(x => x.Key, x => TimeSpan.FromSeconds(x.Value));
            //var longestGame = string.Empty;
            //foreach (var game in longestGameList)
            //    longestGame += $"Length: {game.Value.ToString("c")} Match ID: {game.Key}\n";
            //var shortestGameList = matches.Where(x => x.Duration == matches.Select(x => x.Duration).Min()).Select(x=> new {Key = x.MatchId, Value = x.Duration}).ToDictionary(x => x.Key, x => TimeSpan.FromSeconds(x.Value));
            //var shortestGame = string.Empty;
            //foreach (var game in shortestGameList)
            //    shortestGame += $"Length: {game.Value.ToString("c")} Match ID: {game.Key}\n";

            ////var webInterfaceFactory = new SteamWebInterfaceFactory(_config.Steam.SteamKey);

            ////var steamInterface = webInterfaceFactory.CreateSteamWebInterface<DOTA2Match>(_httpClient);
            ////var matchDetails = await steamInterface.GetMatchDetailsAsync(6707754788);
            ////var playerSummaryData = matchDetails.Data;
            ////var mostDeathCount = playerSummaryData.Players.Select(x => x.Deaths).Max();
            ////var mostDeaths = playerSummaryData.Players.Where(x => x.Deaths == mostDeathCount);
            ////var mostKillsCount = playerSummaryData.Players.Select(x => x.Kills).Max();
            ////var mostKills = playerSummaryData.Players.Where(x => x.Kills == mostKillsCount);
            ////var mostDeathPlayer = string.Empty;
            ////foreach (var player in mostDeaths)
            ////    mostDeathPlayer += player.AccountId + " "; 
            ////var mostKillsPlayer = string.Empty;
            ////foreach (var player in mostKills)
            ////    mostKillsPlayer += player.AccountId + " "; 

            //var heroDeathCounts = matches.SelectMany(x=> x.Teamfights)
            //                             .SelectMany(x => x.Players)
            //                             .Select(x => x.Deaths);

            //var mostPickedIds = heroPickCounts.Where(x => x.Value == heroPickCounts.Values.Max());
            //var mostBannedIds = heroBanCounts.Where(x => x.Value == heroBanCounts.Values.Max());
            //var mostPickedTask = GetHeroNames(mostPickedIds);
            //var mostBannedTask = GetHeroNames(mostBannedIds);
            //var mostPicked = await mostPickedTask + mostPickedIds.First().Value;
            //var mostBanned = await mostBannedTask + mostBannedIds.First().Value;

            //stopwatch.Stop();
            //var totalSeconds = stopwatch.Elapsed.TotalSeconds;
            //await ModifyOriginalResponseAsync(x => x.Content = $"Time Taken: {totalSeconds}s\nMost Picked {mostPicked}\nMost Banned {mostBanned}\n" +
            //                                                   //$"Most Kills: {mostKillsPlayer}\nMost Deaths: {mostDeathPlayer}\n" +
            //                                                   $"Longest game: {longestGame}\nShortest Game: {shortestGame}");
            //await ModifyOriginalResponseAsync(x => x.Content = JsonSerializer.Serialize(matches.First())[..1500]);
        }

        private async Task<string> GetHeroNames(IEnumerable<KeyValuePair<int,int>> values, string locale = "en-GB")
        {
            var names = string.Empty;
            foreach (var value in values)
                names += (await _db.GetEntityInfo<HeroInfoEmbed>(value.Key)).Name + " ";
            return names;
        }

        //[SlashCommand("test", "testing")]
        //public async Task test()
        //{
        //    var webInterfaceFactory = new SteamWebInterfaceFactory(_config.Steam.SteamKey);

        //    var steamInterface = webInterfaceFactory.CreateSteamWebInterface<DOTA2Match>(_httpClient);

        //    var playerSummaryResponse = await steamInterface.GetTeamInfoByTeamIdAsync(7119388, 1);
        //    var playerSummaryData = playerSummaryResponse.Data;
        //    var playerSummaryLastModified = playerSummaryResponse.LastModified;


        //    await RespondAsync(text: JsonSerializer.Serialize(playerSummaryData));
        //}
    }
}
