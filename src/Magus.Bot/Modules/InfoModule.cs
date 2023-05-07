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

        public InfoModule(IAsyncDataService db)
        {
            _db = db;
        }

        [SlashCommand("hero", "I don't need an oracle to know you're not searching for Oracle")]
        public async Task InfoHero([Summary(description: "The heroes name to lookup")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
                                   [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            var heroInfo = (await _db.GetEntityInfo<HeroInfoEmbed>(name, locale ?? Context.Interaction.UserLocale, 1)).FirstOrDefault();

            if (heroInfo != null)
                await RespondAsync(embed: heroInfo.Embed.CreateDiscordEmbed());
            else
                await RespondAsync($"Could not find a hero called **{name}**", ephemeral: true);
        }

        [SlashCommand("ability", "Ahh. How does this one work?")]
        public async Task InfoAbility([Summary(description: "The abilities name to lookup")][Autocomplete(typeof(AbilityAutocompleteHandler))] string name,
                                      [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            var abilityInfo = (await _db.GetEntityInfo<AbilityInfoEmbed>(name, locale ?? Context.Interaction.UserLocale, 1)).FirstOrDefault();

            if (abilityInfo != null)
                await RespondAsync(embed: abilityInfo.Embed.CreateDiscordEmbed());
            else
                await RespondAsync($"Could not find an ability called **{name}**", ephemeral: true);
        }

        [SlashCommand("scepter", "what does Aghanim's Scepter do for this hero?")]
        public async Task InfoScepter([Summary(description: "The heroes name to lookup")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
                                      [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            var heroInfo = (await _db.GetEntityInfo<HeroInfoEmbed>(name, locale ?? Context.Interaction.UserLocale, 1)).FirstOrDefault();
            if (heroInfo == null)
            {
                await RespondAsync($"Could not find an scepter for the hero called **{name}**", ephemeral: true);
                return;
            }

            var abilityInfo = await _db.GetHeroScepter(heroInfo.EntityId, locale ?? Context.Interaction.UserLocale);
            if (abilityInfo != null)
                await RespondAsync(embed: abilityInfo.Embed.CreateDiscordEmbed());
            else
                await RespondAsync($"Could not find an scepter for the hero called **{name}**", ephemeral: true);
        }

        [SlashCommand("shard", "what does Aghanim's Shard do for this hero?")]
        public async Task InfoShard([Summary(description: "The heroes name to lookup")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
                                      [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            var heroInfo = (await _db.GetEntityInfo<HeroInfoEmbed>(name, locale ?? Context.Interaction.UserLocale, 1)).FirstOrDefault();
            if (heroInfo == null)
            {
                await RespondAsync($"Could not find an shard for the hero called **{name}**", ephemeral: true);
                return;
            }

            var abilityInfo = await _db.GetHeroShard(heroInfo.EntityId, locale ?? Context.Interaction.UserLocale);
            if (abilityInfo != null)
                await RespondAsync(embed: abilityInfo.Embed.CreateDiscordEmbed());
            else
                await RespondAsync($"Could not find an shard for the hero called **{name}**", ephemeral: true);
        }

        [SlashCommand("item", "> I need 2211 gold for this and buyback!")]
        public async Task InfoItem([Summary(description: "The items name to lookup")][Autocomplete(typeof(ItemAutocompleteHandler))] string name,
                                   [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            var itemInfo = (await _db.GetEntityInfo<ItemInfoEmbed>(name, locale ?? Context.Interaction.UserLocale)).FirstOrDefault();

            if (itemInfo != null)
                await RespondAsync(embed: itemInfo.Embed.CreateDiscordEmbed());
            else
                await RespondAsync($"Could not find an item called **{name}**", ephemeral: true);
        }
    }
}
