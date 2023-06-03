using Discord;
using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Services;
using Magus.Common.Emotes;
using Magus.Data;
using Magus.Data.Extensions;
using Magus.Data.Models.Embeds;
using Magus.Data.Models.Stratz.Results;
using Microsoft.Extensions.Options;
using MongoDB.Driver.Linq;
using System.Diagnostics;

namespace Magus.Bot.Modules
{
    [Group(GroupName, "Get statistics")]
    [ModuleRegistration(Location.TESTING, isEnabled: true)]
    public class StatsModule : InteractionModuleBase<SocketInteractionContext>
    {
        const string GroupName = "stats";

        private readonly ILogger<StatsModule> _logger;
        private readonly BotSettings _config;
        private readonly IAsyncDataService _db;
        private readonly StratzService _stratz;
        private readonly EntityNameLocalisationService _entityNameLocalisationService;

        public StatsModule(ILogger<StatsModule> logger, IOptions<BotSettings> config, IAsyncDataService db, StratzService stratz, EntityNameLocalisationService entityNameLocalisationService)
        {
            _logger                        = logger;
            _db                            = db;
            _config                        = config.Value;
            _stratz                        = stratz;
            _entityNameLocalisationService = entityNameLocalisationService;
        }

        [SlashCommand("hero", "Get stats playing as a hero.")]
        public async Task Hero(
            [Summary(description: "The heroes name.")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
            [Summary(description: "Friends to include. You can @mention and manually enter Dota account IDs, separated by spaces.")] string? friend = null,
            [Summary(description: "The language/locale of the response.")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            await DeferAsync(true);

            var user = await _db.GetUser(Context.User);
            var heroInfo = (await _db.GetEntityInfo<HeroInfoEmbed>(name, locale ?? Context.Interaction.UserLocale, 1)).FirstOrDefault();

            // Placeholder for testing
            List<long>? friendIds = null;
            try
            {
                if (friend != null)
                    friendIds = new List<long>() { long.Parse(friend) };
            }
            catch { }
            //

            if (heroInfo == null)
            {
                await FollowupAsync($"Could not find a hero called **{name}**", ephemeral: true);
            }
            else if (user.DotaID != null)
            {
                var playerInfo = await _stratz.GetPlayerHeroStats((long)user.DotaID, heroInfo.EntityId, friendIds);

                var heroMatches = playerInfo.HeroesPerformance.Where(x => x.Hero.Id == heroInfo.EntityId).First();

                await FollowupAsync(text: $"Played {heroMatches.MatchCount} matches as **{heroInfo.Name}**, {heroMatches.WinCount} wins.", ephemeral: true);
            }
            else
            {
                await FollowupAsync(text: "No steam set", ephemeral: true);
            }
        }

        [SlashCommand("recent", "Get summary of recent games.")]
        public async Task Recent(
            [Summary(description: "The language/locale of the response.")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            await DeferAsync();

            locale ??= Context.Interaction.UserLocale;

            var user = await _db.GetUser(Context.User);

            if (user.DotaID != null)
            {
                var sw = new Stopwatch();
                var player = (await _stratz.GetRecentStats((long)user.DotaID)).Player;
                sw.Start();
                var summary = player.SimpleSummary;
                var mgroup = player.MatchGroupBySteamId.Single();

                var winPercent = (double) mgroup.WinCount / mgroup.MatchCount * 100;

                var longestMatch  = player.Matches.MaxBy(match => match.DurationSeconds);
                var shortestMatch = player.Matches.MinBy(match => match.DurationSeconds);
                var avgDuration   = player.Matches.Average(match => match.DurationSeconds);

                var awardCount = player.Matches.Where(match => match.Players.Single().Award != QueryRecentResult.PlayerType.MatchType.MatchPlayerType.MatchPlayerAward.NONE)
                    .GroupBy(x => x.Players.Single().Award)
                    .OrderByDescending(x => x.Count())
                    .Select(x=> new { x.Key, Count = x.Count() })
                    .ToList();

                var description = new StringBuilder()
                    .AppendLine($"Last Played: <t:{summary.LastUpdateDateTime}:R>")
                    .AppendLine($"WR: **{winPercent:0.#}%** over **{mgroup.MatchCount}** matches")
                    .AppendLine($"KDA: **{mgroup.AvgKills:0.##}\u202F/\u202F{mgroup.AvgDeaths:0.##}\u202F/\u202F{mgroup.AvgAssists:0.##}**\u2007\u2007\u2007Ratio: **{mgroup.AvgKDA:0.##}**")
                    .AppendLine($"Avg. Duration **{SecondsToTime(avgDuration)}**")
                    .AppendLine()
                    .Append("**Best heroes:**");

                var embed = new EmbedBuilder()
                    .WithAuthor(player.SteamAccount.Name, player.SteamAccount.Avatar, $"https://stratz.com/players/{user.DotaID}")
                    .WithColor(Color.DarkGreen)
                    .WithDescription(description.ToString())
                    .WithFooter($"Powered by STRATZ", "https://cdn.discordapp.com/emojis/1113573151549423657.webp");

                for (var i = 0; i < 3; i++)
                {
                    var hero = summary.Heroes.ElementAtOrDefault(i);
                    if (hero != null)
                        embed.AddField($"{HeroEmotes.GetFromHeroId(hero.HeroId)} {_entityNameLocalisationService.GetLocalisedHeroName(hero.HeroId, locale)}", $"**{hero.WinCount}\u202F-\u202F{hero.LossCount}**", true);
                    else
                        embed.AddField("_ _", "_ _", true);
                }
                // Won as <side>, stomped in <time>
                // Lost to a stomp
                // Won with a comeback
                // Lost to a comeback
                // Won a **close game** as <side>, 
                embed.AddField("Shortest Match", $"{(shortestMatch?.Players.Single().IsVictory ?? false ? "Win" : "Loss")} as {shortestMatch?.Players.Single().HeroId} {shortestMatch?.Id} - {SecondsToTime(shortestMatch?.DurationSeconds ?? 0)} (<t:{shortestMatch?.EndDateTime}:R>)", true);
                embed.AddField("Longest Match", $"{(longestMatch?.Players.Single().IsVictory?? false ? "Win" : "Loss")} as {longestMatch?.Players.Single().HeroId} {longestMatch?.Id} - {SecondsToTime(longestMatch?.DurationSeconds ?? 0)} (<t:{longestMatch?.EndDateTime}:R>)", true);

                sw.Stop();
                _logger.LogDebug(sw.Elapsed.TotalSeconds.ToString());
                await FollowupAsync(embed: embed.Build());
            }
            else
            {
                await FollowupAsync(text: "No steam set", ephemeral: true);
            }
        }


        private static string SecondsToTime(double seconds) => TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss");
    }
}
