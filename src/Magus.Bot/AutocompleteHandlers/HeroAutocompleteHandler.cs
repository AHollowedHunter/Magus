using Discord;
using Discord.Interactions;
using Magus.Data;
using Magus.Data.Models.Embeds;

namespace Magus.Bot.AutocompleteHandlers
{
    public class HeroAutocompleteHandler : AutocompleteHandler
    {
        private readonly IAsyncDataService _db;
        private readonly IServiceProvider _services;

        public HeroAutocompleteHandler(IAsyncDataService db, IServiceProvider services)
        {
            _db = db;
            _services = services;
        }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services)
        {
            try
            {
                var locale = context.Interaction.UserLocale;
                var value = autocompleteInteraction.Data.Current.Value as string;
                List<HeroInfoEmbed> heroes;
                if (string.IsNullOrEmpty(value))
                {
                    heroes = (await _db.GetRecords<HeroInfoEmbed>(locale, 25)).ToList();
                }
                else
                {
                    heroes = (await _db.GetEntityInfo<HeroInfoEmbed>(value, locale, limit: 25)).ToList();
                }

                List<AutocompleteResult> results = new();
                heroes.ForEach(hero => results.Add(new AutocompleteResult(hero.Name, hero.InternalName)));

                return AutocompletionResult.FromSuccess(results);
            }
            catch (Exception ex)
            {
                return AutocompletionResult.FromError(ex);
            }
        }

    }
}
