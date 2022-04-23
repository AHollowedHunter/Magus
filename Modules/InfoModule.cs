using Discord;
using Discord.Interactions;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Extensions;
using Magus.Data;
using Magus.Data.Models;
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

        [SlashCommand("hero", "I need a hero")]
        public async Task InfoHero(
                [Autocomplete(typeof(HeroAutocompleteHandler))] string id)
        {
            HeroInfo heroInfo;
            if (!int.TryParse(id, System.Globalization.NumberStyles.Integer, null, out int heroId))
            {
                try
                {
                    heroInfo = _db.GetEntityInfo<HeroInfo>(id, limit: 1).First();
                }
                catch
                {
                    await RespondAsync("Error parsing id.", ephemeral: true);
                    return;
                }
            }
            else
            {
                heroInfo = _db.GetEntityInfo<HeroInfo>(heroId);
            }

            await RespondAsync(embed: heroInfo.Embed.CreateDiscordEmbed());
        }

        // Disabled for now
        //[SlashCommand("ability", "Ahh. How does this one work?")]
        //public async Task InfoAbility(
        //        [Autocomplete(typeof(AbilityAutocompleteHandler))] string id,
        //        [Summary(description: "show to just to me")] bool only_me = false)
        //{
        //    if (!int.TryParse(id, System.Globalization.NumberStyles.Integer, null, out int ability))
        //    {
        //        ability = _db.SelectRecordsByName<Ability>(id, limit: 1).First().Id;
        //    }

        //    var abilityInfo = _db.SelectAbilityById(ability);

        //    await RespondAsync(embeds: new Embed[] { embed.Build() }, ephemeral: only_me);
        //}

        [SlashCommand("item", "Living in a material world")]
        public async Task InfoItem(
                [Autocomplete(typeof(ItemAutocompleteHandler))] string id)
        {
            ItemInfo itemInfo;
            if (!int.TryParse(id, System.Globalization.NumberStyles.Integer, null, out int itemId))
            {
                try
                {
                    itemInfo = _db.GetEntityInfo<ItemInfo>(id, limit: 1).First();
                }
                catch
                {
                    await RespondAsync("Error parsing id.", ephemeral: true);
                    return;
                }
            }
            else
            {
                itemInfo = _db.GetEntityInfo<ItemInfo>(itemId);
            }

            await RespondAsync(embed: itemInfo.Embed.CreateDiscordEmbed());
        }
    }
}
