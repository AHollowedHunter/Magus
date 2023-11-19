using Discord;
using Discord.Interactions;
using Magus.Common.Options;
using Microsoft.Extensions.Options;

namespace Magus.Bot.AutocompleteHandlers;

public class LocaleAutocompleteHandler : AutocompleteHandler
{
    private readonly LocalisationOptions _localisationOptions;

    public LocaleAutocompleteHandler(IOptions<LocalisationOptions> config)
    {
        _localisationOptions = config.Value;
    }

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        try
        {
            return await Task.Run(() =>
            {
                var value = autocompleteInteraction.Data.Current.Value as string;
                var locales = new List<string>();
                if (string.IsNullOrEmpty(value))
                {
                    locales = _localisationOptions.Locales.Take(25).ToList();
                }
                else
                {
                    locales = _localisationOptions.SourceLocaleMappings.Where(x => x.Key.StartsWith(value) || x.Value.Any(x => x.StartsWith(value)))
                                                                       .SelectMany(x => x.Value)
                                                                       .ToList();
                }
                List<AutocompleteResult> results = new();
                foreach (var locale in locales)
                    results.Add(new AutocompleteResult(locale, locale));
                return AutocompletionResult.FromSuccess(results);
            });
        }
        catch (Exception ex)
        {
            return AutocompletionResult.FromError(ex);
        }
    }
}
