using Discord;
using Discord.Interactions;
using Magus.Data.Models.Dota;
using Magus.Data.Services;

namespace Magus.Bot.AutocompleteHandlers;

public class PatchAutocompleteHandler : AutocompleteHandler
{
    private readonly MeilisearchService _meilisearchService;

    public PatchAutocompleteHandler(MeilisearchService meilisearchService)
    {
        _meilisearchService = meilisearchService;
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
            List<Patch> patches;
            if (string.IsNullOrEmpty(value))
            {
                patches = [await _meilisearchService.GetLatestPatchAsync()];
            }
            else
            {
                patches = (await _meilisearchService.GetPatchesAsync(value, limit: 25, orderByDesc: true)).ToList();
            }

            List<AutocompleteResult> results = [];
            patches.ForEach(patch => results.Add(new AutocompleteResult(patch.PatchNumber, patch.PatchNumber)));

            return AutocompletionResult.FromSuccess(results);
        }
        catch (Exception ex)
        {
            return AutocompletionResult.FromError(ex);
        }
    }

}
