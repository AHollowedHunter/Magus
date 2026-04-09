using Magus.Data.Models.Dota;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UltimyrArchives.Updater.Utils;

namespace UltimyrArchives.Updater;

internal sealed class EntityProcessor(ILogger<EntityProcessor> logger, GameFileProviderFactory gameFileProviderFactory)
{
    public async Task<IReadOnlyList<Entity>> GetProcessedAsync()
    {
        logger.LogInformation("Processing Entities.");
        var stopwatch = Stopwatch.StartNew();

        var entities = await GetEntitiesAsync();

        stopwatch.Stop();
        logger.LogInformation("Finished Processing Entities, took {timeTaken}.", stopwatch.Elapsed);

        return entities;
    }

    private async Task<Entity[]> GetEntitiesAsync()
    {
        LocalisedValues localisedValues;
        
        using (var gameFileProvider = gameFileProviderFactory.Create())
        {
            localisedValues = await new LocalisedValuesBuilder(gameFileProvider)
                .WithAbilities(StringUtils.CleanSimple) // TODO should 'clean' here or later when formatting values? 
                .WithDota(StringUtils.CleanSimple)
                .WithHeroLoreAsync(StringUtils.CleanSimple)
                .BuildAsync()
                .ConfigureAwait(false);
            
        }
        
        // TODO process

        return [];
    }
}
