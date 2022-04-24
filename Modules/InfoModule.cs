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
        public async Task InfoHero([Autocomplete(typeof(HeroAutocompleteHandler))] string id)
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

        //Disabled for now
        //[SlashCommand("ability", "Ahh. How does this one work?")]
        public async Task InfoAbility([Autocomplete(typeof(AbilityAutocompleteHandler))] string id)
        {
            AbilityInfo abilityInfo;
            if (!int.TryParse(id, System.Globalization.NumberStyles.Integer, null, out int abilityId))
            {
                try
                {
                    abilityInfo = _db.GetEntityInfo<AbilityInfo>(id, limit: 1).First();
                }
                catch
                {
                    await RespondAsync("Error parsing id.", ephemeral: true);
                    return;
                }
            }
            else
            {
                abilityInfo = _db.GetEntityInfo<AbilityInfo>(abilityId);
            }

            await RespondAsync(embed: abilityInfo.Embed.CreateDiscordEmbed());
        }

        [SlashCommand("item", "Living in a material world")]
        public async Task InfoItem([Autocomplete(typeof(ItemAutocompleteHandler))] string id)
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
