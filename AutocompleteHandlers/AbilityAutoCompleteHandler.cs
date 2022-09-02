using Discord;
using Discord.Interactions;
using Magus.Data;
using Magus.Data.Models.Embeds;

namespace Magus.Bot.AutocompleteHandlers
{
    public class AbilityAutocompleteHandler : AutocompleteHandler
    {
        private readonly IDatabaseService _db;
        private readonly IServiceProvider _services;

        public AbilityAutocompleteHandler(IDatabaseService db, IServiceProvider services)
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
                var value = autocompleteInteraction.Data.Current.Value as string;
                List<AbilityInfoEmbed> abilites;
                if (string.IsNullOrEmpty(value))
                {
                    abilites = _db.GetRecords<AbilityInfoEmbed>(25).ToList();
                }
                else
                {
                    abilites = _db.GetEntityInfo<AbilityInfoEmbed>(value, limit: 25).ToList();
                }

                List<AutocompleteResult> results = new();
                abilites.ForEach(ability => results.Add(new AutocompleteResult(ability.Name, ability.InternalName)));

                return Task.FromResult(AutocompletionResult.FromSuccess(results));
            }
            catch (Exception ex)
            {
                return Task.FromResult(AutocompletionResult.FromError(ex));
            }
        }

    }
}
