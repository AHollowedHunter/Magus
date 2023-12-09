using Discord;
using Discord.Interactions;
using Magus.Data.Enums;
using Magus.Data.Services;

namespace Magus.Bot.AutocompleteHandlers;

public class HeroAutocompleteHandler : AutocompleteHandler
{
    private MeilisearchService Meilisearch { get; }

    public HeroAutocompleteHandler(MeilisearchService meilisearch)
    {
        Meilisearch = meilisearch;
    }

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        try
        {
            var value = autocompleteInteraction.Data.Current.Value as string;

            var search = await Meilisearch.SearchEntityMetaAsync(value, EntityType.Hero).ConfigureAwait(false);

            List<AutocompleteResult> results = [];
            search.ToList().ForEach(hero => results.Add(new AutocompleteResult(hero.Name["en"], hero.InternalName))); // TODO handle localisation

            return AutocompletionResult.FromSuccess(results);
        }
        catch (Exception ex)
        {
            return AutocompletionResult.FromError(ex);
        }
    }

}
