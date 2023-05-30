using Discord;
using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Services;
using Magus.Data;
using Magus.Data.Extensions;
using Magus.Data.Models.Embeds;
using Microsoft.Extensions.Options;
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
            await DeferAsync(true);

            var user = await _db.GetUser(Context.User);

            if (user.DotaID != null)
            {
                var summary = await _stratz.GetPlayerSummary((long)user.DotaID);

                var embed = new EmbedBuilder()
                    .WithTitle(summary.SteamAccount.Name)
                    .WithDescription($"Last {summary.MatchCount} matches.\n\n**Best heroes:**");

                foreach (var hero in summary.Heroes)
                    embed.AddField(hero.HeroId.ToString(), $"{hero.WinCount} - {hero.LossCount}", true);

                await FollowupAsync(embed: embed.Build(), ephemeral: true);
            }
            else
            {
                await FollowupAsync(text: "No steam set", ephemeral: true);
            }
        }
    }
}
