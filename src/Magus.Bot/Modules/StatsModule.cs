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
using Microsoft.Extensions.Primitives;
using MongoDB.Driver.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Magus.Bot.Modules
{
    [Group(GroupName, "Get statistics")]
    [ModuleRegistration(Location.TESTING, isEnabled: true)]
    public class StatsModule : InteractionModuleBase<SocketInteractionContext>
    {
        const string GroupName = "stats";

        const string WideSpace = "\u2007\u2007\u2007";

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

                var longestMatch  = player.Matches.MaxBy(match => match.DurationSeconds);
                var shortestMatch = player.Matches.MinBy(match => match.DurationSeconds);
                var avgDuration   = player.Matches.Average(match => match.DurationSeconds);

                var awardCount = player.Matches.Where(match => match.Players.Single().Award != QueryRecentResult.PlayerType.MatchType.MatchPlayerType.MatchPlayerAward.NONE)
                    .GroupBy(x => x.Players.Single().Award)
                    .OrderByDescending(x => x.Count())
                    .Select(x=> new { x.Key, Count = x.Count() })
                    .ToList();

                var description = new StringBuilder()
                    .AppendLine($"Last Played: <t:{summary.LastUpdateDateTime}:R> ({MatchIdLink(player.Matches.First().Id)})")
                    .AppendLine(WinRate(mgroup.MatchCount, mgroup.WinCount))
                    .AppendLine(KDA(mgroup, true))
                    .AppendLine($"Avg. Duration **{SecondsToTime(avgDuration)}**{WideSpace}Avg. GPM: **{mgroup.AvgGoldPerMinute:n0}**{WideSpace}Avg. XPM: **{mgroup.AvgExperiencePerMinute:n0}**")
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
                        embed.AddField($"{HeroEmotes.GetFromHeroId(hero.HeroId)} {_entityNameLocalisationService.GetLocalisedHeroName(hero.HeroId, locale)}", HeroSummary(player.MatchGroupByHero.First(x => x.HeroId == hero.HeroId)), true);
                    else
                        embed.AddField("_ _", "_ _", true);
                }

                if (shortestMatch != null)
                    embed.AddField("Shortest Match", MatchSummary(shortestMatch, locale), true);
                if (longestMatch != null)
                    embed.AddField("Longest Match", MatchSummary(longestMatch, locale), true);

                sw.Stop();
                _logger.LogDebug(sw.Elapsed.TotalSeconds.ToString());
                await FollowupAsync(embed: embed.Build());
            }
            else
            {
                await FollowupAsync(text: "No steam set", ephemeral: true);
            }
        }


        private static string SecondsToTime(double seconds) => TimeSpan.FromSeconds(seconds).ToString(@"h\:mm\:ss");

        private static string WinRate(double matchCount, double wins)
        => new StringBuilder()
            .Append("WR: **")
            .Append((wins /matchCount * 100).ToString("0.#"))
            .Append("%** in **")
            .Append(matchCount)
            .Append("** matches")
            .ToString();

        private static string KDA(MatchGroupByType group, bool showRatio = false)
        {
            var sb = new StringBuilder()
                .Append("KDA: **")
                .Append(group.AvgKills.ToString("0.##"))
                .Append("\u202F/\u202F")
                .Append(group.AvgDeaths.ToString("0.##"))
                .Append("\u202F/\u202F")
                .Append(group.AvgAssists.ToString("0.##"))
                .Append("**");
            if (showRatio)
            {
                sb.Append(WideSpace);
                sb.Append("Ratio: **");
                sb.Append(group.AvgKDA.ToString("0.##"));
                sb.Append("**");
            }
            return sb.ToString();
        }

        private static string HeroSummary(QueryRecentResult.PlayerType.MatchGroupByHeroType heroGroup)
        {
            var sb = new StringBuilder()
                .AppendLine(WinRate(heroGroup.MatchCount, heroGroup.WinCount))
                .AppendLine(KDA(heroGroup))
                .Append("GPM: **")
                .Append(heroGroup.AvgGoldPerMinute)
                .AppendLine("**")
                .Append("XPM: **")
                .Append(heroGroup.AvgExperiencePerMinute)
                .Append("**");

            return sb.ToString();
        }

        private string MatchSummary(QueryRecentResult.PlayerType.MatchType match, string locale)
        {
            var sb = new StringBuilder();
            sb.Append(match.Players.Single().IsVictory ? "Won" : "Lost");
            if (match.AnalysisOutcome != QueryRecentResult.PlayerType.MatchType.MatchAnalysisOutcome.NONE)
            {
                sb.Append(' ');
                sb.Append(OutcomeDescription(match.AnalysisOutcome));
            }
            sb.Append(" in ");
            sb.Append(SecondsToTime(match.DurationSeconds));
            sb.Append(" as ");
            sb.Append(HeroEmotes.GetFromHeroId(match!.Players.Single().HeroId));
            sb.Append('\u202F');
            sb.AppendLine(_entityNameLocalisationService.GetLocalisedHeroName(match.Players.Single().HeroId, locale));

            sb.Append("KDA: **");
            sb.Append(match.Players.Single().Kills);
            sb.Append("\u202F/\u202F");
            sb.Append(match.Players.Single().Deaths);
            sb.Append("\u202F/\u202F");
            sb.Append(match.Players.Single().Assists);
            sb.AppendLine("**");

            sb.Append("GPM: **");
            sb.Append(match.Players.Single().GoldPerMinute);
            sb.Append("**");
            sb.Append(WideSpace);
            sb.Append(Emotes.GoldIcon);
            sb.Append('\u202F');
            sb.AppendLine(match.Players.Single().Networth.ToString("n0"));

            sb.Append("XPM: **");
            sb.Append(match.Players.Single().ExperiencePerMinute.ToString("n0"));
            sb.Append("**");
            sb.Append(WideSpace);
            sb.Append("Level: **");
            sb.Append(match.Players.Single().Level);
            sb.AppendLine("**");

            sb.Append(MatchIdLink(match.Id));
            sb.Append(" <t:");
            sb.Append(match.EndDateTime);
            sb.Append(":R>");

            return sb.ToString();
        }

        private static string OutcomeDescription(QueryRecentResult.PlayerType.MatchType.MatchAnalysisOutcome outcome)
            => outcome switch
            {
                QueryRecentResult.PlayerType.MatchType.MatchAnalysisOutcome.STOMPED => "a stomp",
                QueryRecentResult.PlayerType.MatchType.MatchAnalysisOutcome.COMEBACK => "a comeback",
                QueryRecentResult.PlayerType.MatchType.MatchAnalysisOutcome.CLOSE_GAME => "a close game",
                _ => string.Empty
            };

        private static string MatchIdLink(long matchId)
            => new StringBuilder()
            .Append('[')
            .Append(matchId)
            .Append("](https://stratz.com/matches/")
            .Append(matchId)
            .Append(')')
            .ToString();
    }
}
