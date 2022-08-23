using Discord;
using Discord.Interactions;
using Magus.Data;
using Magus.Data.Models.Dota;

namespace Magus.Bot.AutocompleteHandlers
{
    public class PatchAutocompleteHandler : AutocompleteHandler
    {
        private readonly IDatabaseService _db;
        private readonly IServiceProvider _services;

        public PatchAutocompleteHandler(IDatabaseService db, IServiceProvider services)
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
                List<Patch> patches;
                if (string.IsNullOrEmpty(value))
                {
                    patches = new List<Patch>() { _db.GetLatestPatch() };
                }
                else
                {
                    patches = _db.GetPatches(value, limit: 25, orderByDesc: true).ToList();
                }

                List<AutocompleteResult> results = new();
                patches.ForEach(patch => results.Add(new AutocompleteResult(patch.PatchNumber, patch.PatchNumber)));

                return Task.FromResult(AutocompletionResult.FromSuccess(results));
            }
            catch (Exception ex)
            {
                return Task.FromResult(AutocompletionResult.FromError(ex));
            }
        }

    }
}
