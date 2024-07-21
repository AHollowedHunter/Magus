using Discord;
using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Services;
using Magus.Data.Constants;
using Magus.Data.Enums;
using Magus.Data.Services;

namespace Magus.Bot.Modules;

[Group("info", "Get information about specific heroes, abilities, and items.")]
[ModuleRegistration(Location.GLOBAL)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
public class InfoModule : ModuleBase
{
    private readonly LocalisationService _localisationService;
    private readonly MeilisearchService _meilisearchService;

    public InfoModule(LocalisationService entityNameLocalisationService, MeilisearchService meilisearchService)
    {
        _localisationService = entityNameLocalisationService;
        _meilisearchService = meilisearchService;
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
        => await InfoAghanim(name, EntityFilter.Scepter, locale);

    [SlashCommand("shard", "What does Aghanim's Shard do for this hero?")]
    public async Task InfoShard([Summary(description: "The heroes name to lookup")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
                                  [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        => await InfoAghanim(name, EntityFilter.Shard, locale);

    private async Task InfoAghanim(string name, string filter, string? locale = null)
    {

        await DeferAsync();
        locale = _localisationService.LocaleConfirmOrDefault(locale ?? Context.Interaction.UserLocale); // TODO simplify
        var hero = await _meilisearchService.SearchTopEntityAsync(name, EntityType.Hero);
        if (hero is not null)
        {
            var ability = (await _meilisearchService.SearchEntityWithFiltersAsync(hero.InternalName, [filter], EntityType.Ability)).FirstOrDefault();
            if (ability is not null)
            {
                var abilityInfo = await _meilisearchService.GetEntityInfoAsync(ability.InternalName, locale);
                await FollowupAsync(embed: abilityInfo.Embed.ToDiscordEmbed());
                return;
            }
        }
        await FollowupAsync($"Could not find a {filter} for the hero called **{name}**", ephemeral: true);
    }

    [SlashCommand("item", "Get information about an item.")]
    public async Task InfoItem([Summary(description: "The items name to lookup")][Autocomplete(typeof(ItemAutocompleteHandler))] string name,
                               [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        => await RespondWithEntityInfo(name, locale, EntityType.Item);

    private async Task RespondWithEntityInfo(string name, string? locale, EntityType entityType)
    {
        await DeferAsync();
        locale = _localisationService.LocaleConfirmOrDefault(locale ?? Context.Interaction.UserLocale); // TODO simplify
        var entity = await _meilisearchService.SearchTopEntityAsync(name, entityType);
        if (entity is not null)
        {
            var entityInfo = await _meilisearchService.GetEntityInfoAsync(entity.InternalName, locale);
            await FollowupAsync(embed: entityInfo.Embed.ToDiscordEmbed());
        }
        else
        {
            await FollowupAsync($"Could not find {entityType} called **{name}**", ephemeral: true); // TODO Localise
        }
    }
}
