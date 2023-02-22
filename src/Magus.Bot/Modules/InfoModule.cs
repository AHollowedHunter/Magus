﻿using Discord.Interactions;
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

        [SlashCommand("hero", "🎶 I need a hero 🎶")]
        public async Task InfoHero([Summary(description: "The heroes name to lookup")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
                                   [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            var heroInfo = (await _db.GetEntityInfo<HeroInfoEmbed>(name, locale ?? Context.Interaction.UserLocale, 1)).First();

            await RespondAsync(embed: heroInfo.Embed.CreateDiscordEmbed());
        }

        [SlashCommand("ability", "Ahh. How does this one work?")]
        public async Task InfoAbility([Summary(description: "The abilities name to lookup")][Autocomplete(typeof(AbilityAutocompleteHandler))] string name,
                                      [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            var abilityInfo = (await _db.GetEntityInfo<AbilityInfoEmbed>(name, locale ?? Context.Interaction.UserLocale, 1)).First();

            await RespondAsync(embed: abilityInfo.Embed.CreateDiscordEmbed());
        }

        [SlashCommand("item", "Living in a material world")]
        public async Task InfoItem([Summary(description: "The items name to lookup")][Autocomplete(typeof(ItemAutocompleteHandler))] string name,
                                   [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            var itemInfo = (await _db.GetEntityInfo < ItemInfoEmbed >(name, locale ?? Context.Interaction.UserLocale, 1)).First();

            await RespondAsync(embed: itemInfo.Embed.CreateDiscordEmbed());
        }
    }
}