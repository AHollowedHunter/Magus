using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Extensions;
using Magus.Bot.Services;
using Magus.Data.Constants;
using Magus.Data.Enums;
using Magus.Data.Models.Embeds;
using Magus.Data.Models.V2;
using Magus.Data.Services;

namespace Magus.Bot.Modules;

[Group("info", "Get information about specific heroes, abilities, and items.")]
[ModuleRegistration(Location.GLOBAL)]
public class InfoModule : ModuleBase
{
    private readonly IAsyncDataService _db;
    private readonly LocalisationService _localisationService;
    private MeilisearchService MeilisearchService { get; }

    public InfoModule(IAsyncDataService db, LocalisationService entityNameLocalisationService, MeilisearchService meilisearchService)
    {
        _db = db;
        _localisationService = entityNameLocalisationService;
        MeilisearchService = meilisearchService;
    }

    [SlashCommand("hero", "Get information about a hero; including abilities, vision range, stat base+gain, and more.")]
    public async Task InfoHero([Summary(description: "The heroes name to lookup")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
                               [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        => await RespondWithEntityInfo(name, locale, EntityType.Hero);

    [SlashCommand("ability", "Ahh. How does this one work?")]
    public async Task InfoAbility([Summary(description: "The abilities name to lookup")][Autocomplete(typeof(AbilityAutocompleteHandler))] string name,
                                  [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        => await RespondWithEntityInfo(name, locale, EntityType.Ability);

    [SlashCommand("scepter", "What does Aghanim's Scepter do for this hero?")]
    public async Task InfoScepter([Summary(description: "The heroes name to lookup")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
                                  [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
    {
        await DeferAsync();
        locale = _localisationService.LocaleConfirmOrDefault(locale ?? Context.Interaction.UserLocale);
        var heroInfo = (await _db.GetEntityInfo<HeroInfoEmbed>(name, locale, 1)).FirstOrDefault();
        if (heroInfo == null)
        {
            await FollowupAsync($"Could not find an scepter for the hero called **{name}**", ephemeral: true);
            return;
        }

        var abilityInfo = await _db.GetHeroScepter(heroInfo.EntityId, locale ?? Context.Interaction.UserLocale);
        if (abilityInfo != null)
            await FollowupAsync(embed: abilityInfo.Embed.ToDiscordEmbed());
        else
            await FollowupAsync($"Could not find an scepter for the hero called **{name}**", ephemeral: true);
    }

    [SlashCommand("shard", "What does Aghanim's Shard do for this hero?")]
    public async Task InfoShard([Summary(description: "The heroes name to lookup")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
                                  [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
    {
        await DeferAsync();
        locale = _localisationService.LocaleConfirmOrDefault(locale ?? Context.Interaction.UserLocale); // TODO simplify
        var hero = await MeilisearchService.SearchTopEntityAsync(name, EntityType.Hero);
        if (hero is not null)
        {
            var ability = (await MeilisearchService.SearchEntityWithFiltersAsync(hero.InternalName, [EntityFilter.Shard], EntityType.Ability)).FirstOrDefault();
            if (ability is not null)
            {
                var abilityInfo = await MeilisearchService.GetEntityInfoAsync(ability.InternalName, locale);
                await FollowupAsync(embed: abilityInfo.Embed.ToDiscordEmbed());
                return;
            }
        }
        await FollowupAsync($"Could not find a shard for the hero called **{name}**", ephemeral: true);
    }

    [SlashCommand("item", "Get information about an item.")]
    public async Task InfoItem([Summary(description: "The items name to lookup")][Autocomplete(typeof(ItemAutocompleteHandler))] string name,
                               [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        => await RespondWithEntityInfo(name, locale, EntityType.Item);

    private async Task RespondWithEntityInfo(string name, string? locale, EntityType entityType)
    {
        await DeferAsync();
        locale = _localisationService.LocaleConfirmOrDefault(locale ?? Context.Interaction.UserLocale); // TODO simplify
        var hero = await MeilisearchService.SearchTopEntityAsync(name, entityType);
        if (hero is not null)
        {
            var heroInfo = await MeilisearchService.GetEntityInfoAsync(hero.InternalName, locale);
            await FollowupAsync(embed: heroInfo.Embed.ToDiscordEmbed());
        }
        else
        {
            await FollowupAsync($"Could not find {entityType} called **{name}**", ephemeral: true); // TODO Localise
        }
    }
}
