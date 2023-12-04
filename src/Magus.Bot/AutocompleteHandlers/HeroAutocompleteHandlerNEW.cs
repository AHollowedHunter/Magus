using Discord;
using Discord.Interactions;
using Magus.Bot.Services;
using Magus.Data.Models.Embeds;
using Magus.Data.Models.V2;
using Magus.Data.Services;

namespace Magus.Bot.AutocompleteHandlers;

public class HeroAutocompleteHandlerNEW : AutocompleteHandler
{
    private readonly IAsyncDataService _db;
    private readonly IServiceProvider _services;
    private readonly LocalisationService _localisationService;

    private readonly MeilisearchService _meilisearchService = new(); // HACK

    public HeroAutocompleteHandlerNEW(IAsyncDataService db, IServiceProvider services, LocalisationService localisationService)
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
            List<HeroInfoEmbed> heroes;

            var search = await _meilisearchService.SearchIndexAsync<EntityMeta>(value).ConfigureAwait(false); // will need filter

            if (string.IsNullOrEmpty(value))
            {
                heroes = (await _db.GetRecords<HeroInfoEmbed>(locale, 25)).ToList();
            }
            else
            {
                heroes = (await _db.GetEntityInfo<HeroInfoEmbed>(value, locale, limit: 25)).ToList();
            }

            List<AutocompleteResult> results = new();
            search.ToList().ForEach(hero => results.Add(new AutocompleteResult(hero.Name["en"], hero.InternalName)));

            return AutocompletionResult.FromSuccess(results);
        }
        catch (Exception ex)
        {
            return AutocompletionResult.FromError(ex);
        }
    }

}
