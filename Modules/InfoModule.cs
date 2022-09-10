using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Extensions;
using Magus.Data;
using Magus.Data.Models.Embeds;

namespace Magus.Bot.Modules
{
    [Group("info", "Information commands")]
    [ModuleRegistration(Location.GLOBAL)]
    public class InfoModule : ModuleBase
    {
        private readonly IAsyncDataService _db;
        private readonly IServiceProvider _services;

        public InfoModule(IAsyncDataService db, IServiceProvider services)
        {
            _db = db;
            _services = services;
        }

        [SlashCommand("hero", "🎶 I need a hero 🎶")]
        public async Task InfoHero([Autocomplete(typeof(HeroAutocompleteHandler))] string name)
        {
            var heroInfo = (await _db.GetEntityInfo<HeroInfoEmbed>(name, Context.Interaction.UserLocale, 1)).First();

            await RespondAsync(embed: heroInfo.Embed.CreateDiscordEmbed());
        }

        [SlashCommand("ability", "Ahh. How does this one work?")]
        public async Task InfoAbility([Autocomplete(typeof(AbilityAutocompleteHandler))] string name)
        {
            var abilityInfo = (await _db.GetEntityInfo<AbilityInfoEmbed>(name, Context.Interaction.UserLocale, 1)).First();

            await RespondAsync(embed: abilityInfo.Embed.CreateDiscordEmbed());
        }

        [SlashCommand("item", "Living in a material world")]
        public async Task InfoItem([Autocomplete(typeof(ItemAutocompleteHandler))] string name)
        {
            var itemInfo = (await _db.GetEntityInfo < ItemInfoEmbed >(name, Context.Interaction.UserLocale, 1)).First();

            await RespondAsync(embed: itemInfo.Embed.CreateDiscordEmbed());
        }
    }
}
