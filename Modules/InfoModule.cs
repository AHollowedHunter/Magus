using Discord.Interactions;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Extensions;
using Magus.Data;
using Magus.Data.Models.Embeds;

namespace Magus.Bot.Modules
{
    [Group("info", "Information commands")]
    public class InfoModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IDatabaseService _db;
        private readonly IServiceProvider _services;

        public InfoModule(IDatabaseService db, IServiceProvider services)
        {
            _db = db;
            _services = services;
        }

        [SlashCommand("hero", "🎶 I need a hero 🎶")]
        public async Task InfoHero([Autocomplete(typeof(HeroAutocompleteHandler))] int id)
        {
            var heroInfo = _db.GetEntityInfo<HeroInfoEmbed>(id, Context.Interaction.UserLocale);

            await RespondAsync(embed: heroInfo.Embed.CreateDiscordEmbed());
        }

        //Disabled for now
        [SlashCommand("ability", "Ahh. How does this one work?")]
        public async Task InfoAbility([Autocomplete(typeof(AbilityAutocompleteHandler))] int id)
        {
            var abilityInfo = _db.GetEntityInfo<AbilityInfoEmbed>(id, Context.Interaction.UserLocale);

            await RespondAsync(embed: abilityInfo.Embed.CreateDiscordEmbed());
        }

        [SlashCommand("item", "Living in a material world")]
        public async Task InfoItem([Autocomplete(typeof(ItemAutocompleteHandler))] int id)
        {
            var itemInfo = _db.GetEntityInfo<ItemInfoEmbed>(id, Context.Interaction.UserLocale);

            await RespondAsync(embed: itemInfo.Embed.CreateDiscordEmbed());
        }
    }
}
