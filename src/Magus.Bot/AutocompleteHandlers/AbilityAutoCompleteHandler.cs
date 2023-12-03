using Discord;
using Discord.Interactions;
using Magus.Bot.Services;
using Magus.Data.Models.Embeds;
using Magus.Data.Services;

namespace Magus.Bot.AutocompleteHandlers;

public class AbilityAutocompleteHandler : AutocompleteHandler
{
    private readonly IAsyncDataService _db;
    private readonly IServiceProvider _services;
    private readonly LocalisationService _localisationService;

    public AbilityAutocompleteHandler(IAsyncDataService db, IServiceProvider services, LocalisationService localisationService)
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
            List<AbilityInfoEmbed> abilities;
            if (string.IsNullOrEmpty(value))
            {
                abilities = (await _db.GetRecords<AbilityInfoEmbed>(locale, 25)).ToList();
            }
            else
            {
                abilities = (await _db.GetEntityInfo<AbilityInfoEmbed>(value, locale, limit: 25)).ToList();
            }

            List<AutocompleteResult> results = new();
            abilities.ForEach(ability => results.Add(new AutocompleteResult(ability.Name, ability.InternalName)));

            return AutocompletionResult.FromSuccess(results);
        }
        catch (Exception ex)
        {
            return AutocompletionResult.FromError(ex);
        }
    }

}
