using Magus.Common.Dota.ModelsV2;
using Magus.Data.Enums;
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
        var abilityConverter    = new AbilityConverter(baseAbility, abilityIds);

        // NOTE these include generic talents, e.g. special_bonus_agility_15
        List<UnitAbility> unitAbilities = ConvertEntity(
            npcAbilities.Root.Where(x => x.Value != baseAbility && x.Key is not "Version"),
            EntityType.Ability,
            abilityConverter.ConvertUnitAbility);
        List<Item> items = ConvertEntity(itemFile.Root.Where(x => x.Key is not "Version"), EntityType.Item, abilityConverter.ConvertItem);


        var heroObjects    = await gameFileProvider.GetPak01KVFileAsync(Pak01.NpcHeroes, new KVSerializerOptions { HasEscapeSequences = true });
        var baseHeroObject = heroObjects.GetSingleValue(InternalName.HeroBase);
        var heroConverter  = new HeroConverter(baseHeroObject);
        List<Hero> heroes = ConvertEntity(
            heroObjects.Root.Where(x => x.Value.GetBooleanOrDefault("Enabled", false, CultureInfo.InvariantCulture)),
            EntityType.Hero,
            heroConverter.Convert);
        List<UnitAbility> heroAbilities = [];
        foreach (var hero in heroes)
        {
            var heroAbilityFile = await gameFileProvider.GetPak01KVFileAsync(Pak01.GetHeroAbilities(hero.InternalName));
            heroAbilities.AddRange(ConvertEntity(heroAbilityFile.Root.Where(x => x.Key is not "Version"), EntityType.Ability, abilityConverter.ConvertUnitAbility));
        }
        // END TEST


        // TODO process

        return [];
    }

    private List<TEntity> ConvertEntity<TEntity>(IEnumerable<KVOPair> entities, EntityType entityType, Func<string, KVObject, TEntity> converter)
    {
        List<TEntity> converted = [];
        foreach ((string name, KVObject entity) in entities)
        {
            if (entity.Count is 0)
            {
                logger.EntityNoChildren(name, entityType);
                continue;
            }

            try
            {
                converted.Add(converter(name, entity));
            }
            catch (Exception ex)
            {
                logger.EntityParsingError(name, entityType, ex);
            }
        }

        return converted;
    }
}
