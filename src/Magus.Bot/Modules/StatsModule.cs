using Discord;
using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Services;
using Magus.Data;
using Magus.Data.Extensions;
using Magus.Data.Models.Embeds;
using Microsoft.Extensions.Options;
using ReverseMarkdown.Converters;
using SteamWebAPI2.Models;

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

        public StatsModule(ILogger<StatsModule> logger, IOptions<BotSettings> config, IAsyncDataService db, StratzService stratz)
        {
            _logger = logger;
            _db = db;
            _config = config.Value;
            _stratz = stratz;
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

        [SlashCommand("summary", "Get summary of recent games.")]
        public async Task Summary(
            [Summary(description: "The language/locale of the response.")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            await DeferAsync();

            var user = await _db.GetUser(Context.User);

            if (user.DotaID != null)
            {
                var player = await _stratz.GetPlayerSummary((long)user.DotaID);
                var summary = player.SimpleSummary;

                var longestMatch = player.Matches.MaxBy(match => match.DurationSeconds);
                var shortestMatch = player.Matches.MinBy(match => match.DurationSeconds);
                var avgDuration = player.Matches.Average(match => match.DurationSeconds);

                var embed = new EmbedBuilder()
                    .WithAuthor(summary.SteamAccount.Name, summary.SteamAccount.Avatar, summary.SteamAccount.ProfileUri)
                    .WithThumbnailUrl("https://static.wikia.nocookie.net/dota2_gamepedia/images/2/25/SeasonalRank5-4.png")
                    .WithColor(Color.DarkPurple)
                    .WithDescription($"Last {summary.MatchCount} matches.\nLast Match: <t:{summary.LastUpdateDateTime}:R>\n\n**Best recent heroes:**")
                    .WithFooter($"Powered by STRATZ    |    Account ID: {user.DotaID}", "https://cdn.discordapp.com/emojis/1113573151549423657.webp");

                embed.AddField("Shortest Match", $"{(shortestMatch?.Players[0].IsVictory?? false ? "Win" : "Loss")} as {shortestMatch?.Players[0].HeroId} {shortestMatch?.Id} - {SecondsToTime(shortestMatch?.DurationSeconds ?? 0)} (<t:{shortestMatch?.EndDateTime}:R>)");
                embed.AddField("Longest Match", $"{(longestMatch?.Players[0].IsVictory?? false ? "Win" : "Loss")} as {longestMatch?.Players[0].HeroId} {longestMatch?.Id} - {SecondsToTime(longestMatch?.DurationSeconds ?? 0)} (<t:{longestMatch?.EndDateTime}:R>)");

                foreach (var hero in summary.Heroes)
                    embed.AddField(hero.HeroId.ToString(), $"{hero.WinCount} - {hero.LossCount}", true);

                await FollowupAsync(embed: embed.Build());
            }
            else
            {
                await FollowupAsync(text: "No steam set", ephemeral: true);
            }
        }

        private string SecondsToTime(int seconds)
        {
            var timespan = TimeSpan.FromSeconds(seconds);
            return timespan.ToString(@"hh\:mm\:ss");
        }
    }
}
