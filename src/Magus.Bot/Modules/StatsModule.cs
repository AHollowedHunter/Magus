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
using Magus.Data.Models.Stratz.Types;
using Magus.Data.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver.Linq;
using System.Text.RegularExpressions;

namespace Magus.Bot.Modules;

[Group(GroupName, "Get statistics")]
[ModuleRegistration(Location.GLOBAL)]
public class StatsModule : InteractionModuleBase<SocketInteractionContext>
{
    const string GroupName = "stats";

    const string WideSpace = "\u2007\u2007\u2007";

    private readonly ILogger<StatsModule> _logger;
    private readonly BotSettings _config;
    private readonly IAsyncDataService _db;
    private readonly StratzService _stratz;
    private readonly LocalisationService _localisationService;

    public StatsModule(ILogger<StatsModule> logger, IOptions<BotSettings> config, IAsyncDataService db, StratzService stratz, LocalisationService localisationService)
    {
        _logger = logger;
        _db = db;
        _config = config.Value;
        _stratz = stratz;
        _localisationService = localisationService;
    }

    //[SlashCommand("hero", "Get stats playing as a hero.")]
    public async Task Hero(
        [Summary(description: "The heroes name.")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
        [Summary(description: "The language/locale of the response.")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
    {
        await DeferAsync(true);

        var user = await _db.GetUser(Context.User);
        var heroInfo = (await _db.GetEntityInfo<HeroInfoEmbed>(name, locale ?? Context.Interaction.UserLocale, 1)).FirstOrDefault();


        if (heroInfo == null)
        {
            await FollowupAsync($"Could not find a hero called **{name}**", ephemeral: true);
        }
        else if (user.DotaID != null)
        {
            var playerInfo = await _stratz.GetPlayerHeroStats((long)user.DotaID, heroInfo.EntityId);

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

        locale = _localisationService.LocaleConfirmOrDefault(locale ?? Context.Interaction.UserLocale);

        var user = await _db.GetUser(Context.User);

        if (user.DotaID != null)
        {
            var player = (await _stratz.GetRecentStats((long)user.DotaID)).Player;
            if (player.SteamAccount == null)
            {
                await FollowupAsync(embed: new EmbedBuilder()
                    .WithTitle("No Dota info for this account.")
                    .WithDescription("If your account is new or you only just started playing Dota please play at least one match, wait a few hours, and try again.")
                    .WithColor(Color.LightOrange)
                    .Build());
                return;
            }
            if (player.SteamAccount.IsAnonymous)
            {
                await FollowupAsync(embed: new EmbedBuilder()
                        .WithTitle("YOUR DOTA ACCOUNT IS PRIVATE")
                        .WithDescription("**You won't be able to see any match data while private.**\n\n" +
                        "Please open Dota 2 and change your settings to enable \"Expose Public Match Data\".\n\n" +
                        "Then play a game, or log in to [STRATZ](https://stratz.com/settings) and go to \"Settings\" and click \"Check My Status\"\n\n" +
                        "**You will need to wait a few hours to a day for new stats to update.**\n\n" +
                        "You do not need to set your Steam account via this command again, unless you linked the wrong one.")
                        .WithImageUrl("https://i.imgur.com/cBmEY44.png")
                        .WithColor(Color.Red)
                        .Build(), ephemeral: true);
                return;
            }
            if (player.Matches.Count == 0)
            {
                await FollowupAsync(embed: new EmbedBuilder()
                    .WithTitle("No Parsed Matches")
                    .WithDescription("No recent All Pick games played and parsed. If you have just finished a game, please wait a bit and try again.")
                    .WithColor(Color.LightOrange)
                    .Build());
                return;
            }
            var summary = player.SimpleSummary;
            var userGroup = player.MatchGroupBySteamId.Single();

            var longestMatch  = player.Matches.MaxBy(match => match.DurationSeconds);
            var shortestMatch = player.Matches.MinBy(match => match.DurationSeconds);
            var avgDuration   = player.Matches.Average(match => match.DurationSeconds);

            var awardCount = player.Matches.Where(match => match.Players.Single().Award != StatsRecentResult.PlayerType.MatchType.MatchPlayerType.MatchPlayerAward.NONE)
                .GroupBy(x => x.Players.Single().Award)
                .OrderByDescending(x => x.Count())
                .Select(x=> new { x.Key, Count = x.Count() })
                .ToList();

            var description = new StringBuilder()
                .AppendLine($"Last Played: <t:{summary.LastUpdateDateTime}:R> ({MatchIdLink(player.Matches.First().Id)})")
                .AppendLine(WinRate(userGroup.MatchCount, userGroup.WinCount))
                .AppendLine(KDA(userGroup, true))
                .AppendLine($"Avg. Duration **{SecondsToTime(avgDuration)}**{WideSpace}Avg. GPM: **{userGroup.AvgGoldPerMinute:n0}**{WideSpace}Avg. XPM: **{userGroup.AvgExperiencePerMinute:n0}**")
                .AppendLine()
                .Append("**Top hero averages**");

            var embed = new EmbedBuilder()
                .WithAuthor(player.SteamAccount.Name, player.SteamAccount.Avatar, $"https://stratz.com/players/{user.DotaID}")
                .WithColor(Color.DarkGreen)
                .WithDescription(description.ToString())
                .WithFooter($"Powered by STRATZ", "https://cdn.discordapp.com/emojis/1113573151549423657.webp");

            for (var i = 0; i < 3; i++)
            {
                var hero = summary.Heroes.ElementAtOrDefault(i);
                if (hero != null)
                    embed.AddField($"{HeroEmotes.GetFromHeroId(hero.HeroId)} {_localisationService.GetLocalisedHeroName(hero.HeroId, locale)}"
                        , HeroSummary(player.MatchGroupByHero.First(x => x.HeroId == hero.HeroId))
                        , true);
                else
                    embed.AddField("_ _", "_ _", true);
            }

            if (shortestMatch != null)
                embed.AddField("Shortest Match", MatchSummary(shortestMatch, locale), true);
            if (longestMatch != null)
                embed.AddField("Longest Match", MatchSummary(longestMatch, locale), true);

            await FollowupAsync(embed: embed.Build());
        }
        else
        {
            await FollowupAsync(embed: await NoSteamMessage());
        }
    }

    private async Task<Discord.Embed> NoSteamMessage()
    {
        IEnumerable<IApplicationCommand> commands = Bot.IsDebug() ? await Context.Guild.GetApplicationCommandsAsync() : await Context.Client.Rest.GetGlobalApplicationCommands();
        var configUserCommandId = commands.Single(c => c.Name == ConfigUserModule.GroupName).Id;
        var commandLink = new StringBuilder()
            .Append("</")
            .Append(ConfigUserModule.GroupName)
            .Append(' ')
            .Append(ConfigUserModule.SteamGroup.SubGroupName)
            .Append(' ')
            .Append("set")
            .Append(':')
            .Append(configUserCommandId)
            .Append('>')
            .ToString();
        return new EmbedBuilder()
            .WithTitle("No Steam account linked")
            .WithDescription($"You need to link a steam account with {commandLink} to use this command. Please enter your Steam ID/Dota Friend ID into this command.")
            .WithColor(Color.Red)
            .Build();
    }

    private static string SecondsToTime(double seconds) => TimeSpan.FromSeconds(seconds).ToString(@"h\:mm\:ss");

    private static string WinRate(double matchCount, double wins)
    => new StringBuilder()
        .Append("WR: **")
        .Append((wins / matchCount * 100).ToString("0.#"))
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

    private static string HeroSummary(StatsRecentResult.PlayerType.MatchGroupByHeroType heroGroup)
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

    private string MatchSummary(StatsRecentResult.PlayerType.MatchType match, string locale)
    {
        var sb = new StringBuilder();
        sb.Append(match.Players.Single().IsVictory ? "Won" : "Lost");
        if (match.AnalysisOutcome != null && match.AnalysisOutcome != StatsRecentResult.PlayerType.MatchType.MatchAnalysisOutcome.NONE)
        {
            sb.Append(' ');
            sb.Append(OutcomeDescription(match.AnalysisOutcome.Value));
        }
        sb.Append(" in ");
        sb.Append(SecondsToTime(match.DurationSeconds));
        sb.Append(" as ");
        sb.Append(HeroEmotes.GetFromHeroId(match!.Players.Single().HeroId));
        sb.Append('\u202F');
        sb.AppendLine(_localisationService.GetLocalisedHeroName(match.Players.Single().HeroId, locale));

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

    private static string OutcomeDescription(StatsRecentResult.PlayerType.MatchType.MatchAnalysisOutcome outcome)
        => outcome switch
        {
            StatsRecentResult.PlayerType.MatchType.MatchAnalysisOutcome.STOMPED => "a stomp",
            StatsRecentResult.PlayerType.MatchType.MatchAnalysisOutcome.COMEBACK => "a comeback",
            StatsRecentResult.PlayerType.MatchType.MatchAnalysisOutcome.CLOSE_GAME => "a close game",
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
