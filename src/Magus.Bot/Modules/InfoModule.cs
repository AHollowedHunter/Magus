using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Extensions;
using Magus.Data;
using Magus.Data.Models.Embeds;

namespace Magus.Bot.Modules
{
    [Group("info", "Get information about specific heroes, abilities, and items.")]
    [ModuleRegistration(Location.GLOBAL)]
    public class InfoModule : ModuleBase
    {
        private readonly IAsyncDataService _db;

        public InfoModule(IAsyncDataService db)
        {
            _db = db;
        }

        [SlashCommand("hero", "Get information about a hero; including abilities, vision range, stat base+gain, and more.")]
        public async Task InfoHero([Summary(description: "The heroes name to lookup")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
                                   [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            await DeferAsync();
            var heroInfo = (await _db.GetEntityInfo<HeroInfoEmbed>(name, locale ?? Context.Interaction.UserLocale, 1)).FirstOrDefault();

            if (heroInfo != null)
                await FollowupAsync(embed: heroInfo.Embed.CreateDiscordEmbed());
            else
                await FollowupAsync($"Could not find a hero called **{name}**", ephemeral: true);
        }

        [SlashCommand("ability", "Ahh. How does this one work?")]
        public async Task InfoAbility([Summary(description: "The abilities name to lookup")][Autocomplete(typeof(AbilityAutocompleteHandler))] string name,
                                      [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            await DeferAsync();
            var abilityInfo = (await _db.GetEntityInfo<AbilityInfoEmbed>(name, locale ?? Context.Interaction.UserLocale, 1)).FirstOrDefault();

            if (abilityInfo != null)
                await FollowupAsync(embed: abilityInfo.Embed.CreateDiscordEmbed());
            else
                await FollowupAsync($"Could not find an ability called **{name}**", ephemeral: true);
        }

        [SlashCommand("scepter", "What does Aghanim's Scepter do for this hero?")]
        public async Task InfoScepter([Summary(description: "The heroes name to lookup")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
                                      [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            await DeferAsync();
            var heroInfo = (await _db.GetEntityInfo<HeroInfoEmbed>(name, locale ?? Context.Interaction.UserLocale, 1)).FirstOrDefault();
            if (heroInfo == null)
            {
                await FollowupAsync($"Could not find an scepter for the hero called **{name}**", ephemeral: true);
                return;
            }

            var abilityInfo = await _db.GetHeroScepter(heroInfo.EntityId, locale ?? Context.Interaction.UserLocale);
            if (abilityInfo != null)
                await FollowupAsync(embed: abilityInfo.Embed.CreateDiscordEmbed());
            else
                await FollowupAsync($"Could not find an scepter for the hero called **{name}**", ephemeral: true);
        }

        [SlashCommand("shard", "What does Aghanim's Shard do for this hero?")]
        public async Task InfoShard([Summary(description: "The heroes name to lookup")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
                                      [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            await DeferAsync();
            var heroInfo = (await _db.GetEntityInfo<HeroInfoEmbed>(name, locale ?? Context.Interaction.UserLocale, 1)).FirstOrDefault();
            if (heroInfo == null)
            {
                await FollowupAsync($"Could not find an shard for the hero called **{name}**", ephemeral: true);
                return;
            }

            var abilityInfo = await _db.GetHeroShard(heroInfo.EntityId, locale ?? Context.Interaction.UserLocale);
            if (abilityInfo != null)
                await FollowupAsync(embed: abilityInfo.Embed.CreateDiscordEmbed());
            else
                await FollowupAsync($"Could not find an shard for the hero called **{name}**", ephemeral: true);
        }

        [SlashCommand("item", "Get information about an item.")]
        public async Task InfoItem([Summary(description: "The items name to lookup")][Autocomplete(typeof(ItemAutocompleteHandler))] string name,
                                   [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            await DeferAsync();
            var itemInfo = (await _db.GetEntityInfo<ItemInfoEmbed>(name, locale ?? Context.Interaction.UserLocale)).FirstOrDefault();

            if (itemInfo != null)
                await FollowupAsync(embed: itemInfo.Embed.CreateDiscordEmbed());
            else
                await FollowupAsync($"Could not find an item called **{name}**", ephemeral: true);
        }
    }
}
