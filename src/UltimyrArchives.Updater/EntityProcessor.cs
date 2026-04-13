using Magus.Common.Dota.ModelsV2;
using Magus.Data.Models.Dota;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UltimyrArchives.Updater.Constants;
using UltimyrArchives.Updater.Converters;
using UltimyrArchives.Updater.DotaFilePaths;
using UltimyrArchives.Updater.Extensions;
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

        using var gameFileProvider = gameFileProviderFactory.Create();
        localisedValues = await new LocalisedValuesBuilder(gameFileProvider)
            .WithAbilities(StringUtils.CleanSimple) // TODO should 'clean' here or later when formatting values? 
            .WithDota(StringUtils.CleanSimple)
            .WithHeroLoreAsync(StringUtils.CleanSimple)
            .BuildAsync()
            .ConfigureAwait(false);

        // FROM TEST

        // Test abilities  
        var kvSerializerOptions = new KVSerializerOptions { HasEscapeSequences = true };
        var abilityIds          = await gameFileProvider.GetPak01KVFileAsync(Pak01.NpcAbilityIds, kvSerializerOptions);
        var npcAbilities        = await gameFileProvider.GetPak01KVFileAsync(Pak01.NpcAbilities, kvSerializerOptions);
        var itemFile            = await gameFileProvider.GetPak01KVFileAsync(Pak01.Items, kvSerializerOptions);
        var baseAbility         = npcAbilities.GetSingleValue(InternalName.AbilityBase);
        var abilityConverter    = new UnitAbilityConverter(baseAbility, abilityIds);
        var itemConverter       = new ItemConverter(baseAbility, abilityIds);

        Dictionary<string, UnitAbility> unitAbilities = ConvertEntity(
            npcAbilities.Root.Where(x => x.Value != baseAbility && x.Key is not "Version"),
            abilityConverter);
        Dictionary<string, Item> items = ConvertEntity(itemFile.Root.Where(x => x.Key is not "Version"), itemConverter);


        var heroObjects    = await gameFileProvider.GetPak01KVFileAsync(Pak01.NpcHeroes, new KVSerializerOptions { HasEscapeSequences = true });
        var baseHeroObject = heroObjects.GetSingleValue(InternalName.HeroBase);
        var heroConverter  = new HeroConverter(baseHeroObject);
        Dictionary<string, Hero> heroes = ConvertEntity(
            heroObjects.Root.Where(x => x.Value.GetBooleanOrDefault("Enabled", false, CultureInfo.InvariantCulture)),
            heroConverter);
        foreach ((string heroName, _) in heroes)
        {
            var heroAbilityFile = await gameFileProvider.GetPak01KVFileAsync(Pak01.GetHeroAbilities(heroName));
            unitAbilities.AddRange(
                ConvertEntity(heroAbilityFile.Root.Where(x => x.Key is not "Version"), abilityConverter));
        }
        // END TEST


        // TODO process

        return [];
    }

    private Dictionary<string, TEntity> ConvertEntity<TEntity>(IEnumerable<KVOPair> entities, IKVObjectConverter<TEntity> converter)
    {
        Dictionary<string, TEntity> converted = [];
        foreach ((string name, KVObject entity) in entities)
        {
            if (entity.Count is 0)
            {
                logger.EntityNoChildren(name, typeof(TEntity).Name);
                continue;
            }

            try
            {
                if (!converted.TryAdd(name, converter.Convert(name, entity)))
                    logger.EntityDuplicate(name, typeof(TEntity).Name);
            }
            catch (Exception ex)
            {
                logger.EntityParsingError(name, typeof(TEntity).Name, ex);
            }
        }

        return converted;
    }
}
