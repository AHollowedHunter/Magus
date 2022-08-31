using Discord;
using Discord.Interactions;
using Magus.Data;
using Magus.Data.Models.Embeds;

namespace Magus.Bot.AutocompleteHandlers
{
    public class HeroAutocompleteHandler : AutocompleteHandler
    {
        private readonly IDatabaseService _db;
        private readonly IServiceProvider _services;

        public HeroAutocompleteHandler(IDatabaseService db, IServiceProvider services)
        {
            _db = db;
            _services = services;
        }

        public override Task<AutocompletionResult> GenerateSuggestionsAsync(
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
                    heroes = _db.GetRecords<HeroInfoEmbed>(locale, 25).ToList();
                }
                else
                {
                    heroes = _db.GetEntityInfo<HeroInfoEmbed>(value, locale, limit: 25).ToList();
                }

                List<AutocompleteResult> results = new();
                heroes.ForEach(hero => results.Add(new AutocompleteResult(hero.Name, hero.EntityId)));

                return Task.FromResult(AutocompletionResult.FromSuccess(results));
            }
            catch (Exception ex)
            {
                return Task.FromResult(AutocompletionResult.FromError(ex));
            }
        }

    }
}
