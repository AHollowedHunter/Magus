using Discord;
using Discord.Interactions;
using Magus.Bot.Services;
using Magus.Data;
using Magus.Data.Models.Embeds;

namespace Magus.Bot.AutocompleteHandlers
{
    public class ItemAutocompleteHandler : AutocompleteHandler
    {
        private readonly IAsyncDataService _db;
        private readonly IServiceProvider _services;

        private readonly LocalisationService _localisationService;

        public ItemAutocompleteHandler(IAsyncDataService db, IServiceProvider services, LocalisationService localisationService)
        {
            _db = db;
            _services = services;

            _localisationService = localisationService;
        }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services)
        {
            try
            {
                var locale = _localisationService.LocaleConfirmOrDefault(context.Interaction.UserLocale);
                var value = autocompleteInteraction.Data.Current.Value as string;
                List<ItemInfoEmbed> items;
                if (string.IsNullOrEmpty(value))
                {
                    items = (await _db.GetRecords<ItemInfoEmbed>(locale, 25)).ToList();
                }
                else
                {
                    items = (await _db.GetEntityInfo<ItemInfoEmbed>(value, locale, limit: 25)).ToList();
                }

                List<AutocompleteResult> results = new();
                items.ForEach(item => results.Add(new AutocompleteResult(item.Name, item.InternalName)));

                return AutocompletionResult.FromSuccess(results);
            }
            catch (Exception ex)
            {
                return AutocompletionResult.FromError(ex);
            }
        }

    }
}
