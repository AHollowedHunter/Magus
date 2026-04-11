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
        var abilityIds       = await gameFileProvider.GetPak01KVFileAsync(Pak01.NpcAbilityIds, new KVSerializerOptions { HasEscapeSequences = true });
        var npcAbilities     = await gameFileProvider.GetPak01KVFileAsync(Pak01.NpcAbilities, new KVSerializerOptions { HasEscapeSequences  = true });
        var itemFile         = await gameFileProvider.GetPak01KVFileAsync(Pak01.Items, new KVSerializerOptions { HasEscapeSequences         = true });
        var baseAbility      = npcAbilities.GetSingleValue(InternalName.AbilityBase);
        var abilityConverter = new AbilityConverter(baseAbility, abilityIds);

        // NOTE these include generic talents, e.g. special_bonus_agility_15
        List<UnitAbility> unitAbilities = [];
        foreach (var abilityPair in npcAbilities.Root.Where(x => x.Value != baseAbility && x.Key is not "Version"))
        {
            (string abilityName, KVObject ability) = abilityPair;
            if (ability.Count == 0)
            {
                // TODO have preset filter, log any that aren't caught.  
                logger.LogInformation("No children for ability: {ability}", abilityName);
                continue;
            }

            try
            {
                unitAbilities.Add(abilityConverter.ConvertUnitAbility(abilityPair));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed parsing ability: {ability}", abilityName);
            }
        }

        List<Item> items = [];
        foreach (var itemPair in itemFile.Root.Where(x => x.Key is not "Version"))
        {
            (string itemName, KVObject item) = itemPair;
            if (!item.Children.Any())
            {
                // TODO have preset filter, log any that aren't caught.  
                logger.LogInformation("No children for item: {item}", itemName);
                continue;
            }

            try
            {
                items.Add(abilityConverter.ConvertItem(itemPair));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed parsing item: {item}", itemName);
            }
        }


        var heroObjects    = await gameFileProvider.GetPak01KVFileAsync(Pak01.NpcHeroes, new KVSerializerOptions { HasEscapeSequences = true });
        var baseHeroObject = heroObjects.GetSingleValue(InternalName.HeroBase);
        var heroConverter  = new HeroConverter(baseHeroObject);
        var heroes = heroObjects.Root.Where(x => x.Value.GetBooleanOrDefault("Enabled", false, CultureInfo.InvariantCulture))
            .Select(heroConverter.Convert)
            .ToList();

        List<UnitAbility> heroAbilities = [];
        foreach (var hero in heroes)
        {
            var heroAbilityFile = await gameFileProvider.GetPak01KVFileAsync(Pak01.GetHeroAbilities(hero.InternalName));
            foreach (var abilityPair in heroAbilityFile.Root.Where(x => x.Key is not "Version"))
            {
                (string abilityName, KVObject ability) = abilityPair;
                if (!ability.Children.Any())
                {
                    // TODO have preset filter, log any that aren't caught.  
                    logger.LogInformation("No children for {hero} ability: {ability}", hero.InternalName, abilityName);
                    continue;
                }

                try
                {
                    heroAbilities.Add(abilityConverter.ConvertUnitAbility(abilityPair));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed parsing hero ability: {ability}", abilityName);
                }
            }
        }
        // END TEST


        // TODO process

        return [];
    }
}
