using Discord;
using Discord.Interactions;
using Magus.Data.Enums;
using Magus.Data.Services;

namespace Magus.Bot.AutocompleteHandlers;

public abstract class EntityAutocompleteHandler : AutocompleteHandler
{
    internal abstract EntityType EntityType { get; }

    private MeilisearchService Meilisearch { get; }

    public EntityAutocompleteHandler(MeilisearchService meilisearch)
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
            var entities = await Meilisearch.SearchEntityAsync(value, this.EntityType).ConfigureAwait(false);

            List<AutocompleteResult> results = [];
            foreach (var entity in entities)
            {
                results.Add(new AutocompleteResult(entity.Name["en"], entity.InternalName)); // TODO handle localisation
            }
            return AutocompletionResult.FromSuccess(results);
        }
        catch (Exception ex)
        {
            return AutocompletionResult.FromError(ex);
        }
    }

}
