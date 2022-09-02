using Discord;
using Discord.Interactions;
using Magus.Data;
using Magus.Data.Models.Embeds;

namespace Magus.Bot.AutocompleteHandlers
{
    public class ItemAutocompleteHandler : AutocompleteHandler
    {
        private readonly IDatabaseService _db;
        private readonly IServiceProvider _services;

        public ItemAutocompleteHandler(IDatabaseService db, IServiceProvider services)
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
                List<ItemInfoEmbed> items;
                if (string.IsNullOrEmpty(value))
                {
                    items = _db.GetRecords<ItemInfoEmbed>(locale, 25).ToList();
                }
                else
                {
                    items = _db.GetEntityInfo<ItemInfoEmbed>(value, locale, limit: 25).ToList();
                }

                List<AutocompleteResult> results = new();
                items.ForEach(item => results.Add(new AutocompleteResult(item.Name, item.InternalName)));

                return Task.FromResult(AutocompletionResult.FromSuccess(results));
            }
            catch (Exception ex)
            {
                return Task.FromResult(AutocompletionResult.FromError(ex));
            }
        }

    }
}
