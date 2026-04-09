using Magus.Common.Dota.ModelsV2;
using Magus.Data.Models.Dota;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UltimyrArchives.Updater.Constants;
using UltimyrArchives.Updater.Converters;
using UltimyrArchives.Updater.DotaFilePaths;
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
        var baseAbility      = npcAbilities.Single(x => x.Name == InternalName.AbilityBase);
        var abilityConverter = new AbilityConverter(baseAbility, abilityIds);

        // NOTE these include generic talents, e.g. special_bonus_agility_15
        List<UnitAbility> unitAbilities = [];
        foreach (var ability in npcAbilities.Where(x => x != baseAbility && x.Name is not "Version"))
        {
            if (!ability.Children.Any())
            {
                // TODO have preset filter, log any that aren't caught.  
                logger.LogInformation("No children for ability: {ability}", ability.Name);
                continue;
            }

            try
            {
                unitAbilities.Add(abilityConverter.ConvertUnitAbility(ability));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed parsing ability: {ability}", ability.Name);
            }
        }

        List<Item> items = [];
        foreach (var item in itemFile.Where(x => x.Name is not "Version"))
        {
            if (!item.Children.Any())
            {
                // TODO have preset filter, log any that aren't caught.  
                logger.LogInformation("No children for item: {item}", item.Name);
                continue;
            }

            try
            {
                items.Add(abilityConverter.ConvertItem(item));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed parsing item: {item}", item.Name);
            }
        }


        var heroObjects    = await gameFileProvider.GetPak01KVFileAsync(Pak01.NpcHeroes, new KVSerializerOptions { HasEscapeSequences = true });
        var baseHeroObject = heroObjects.Single(x => x.Name == InternalName.HeroBase);
        var heroConverter  = new HeroConverter(baseHeroObject);
        var heroes         = heroObjects.Where(x => x["Enabled"]?.ToBoolean(CultureInfo.InvariantCulture) ?? false).Select(heroConverter.Convert).ToList();

        List<UnitAbility> heroAbilities = [];
        foreach (var hero in heroes)
        {
            var heroAbilityFile = await gameFileProvider.GetPak01KVFileAsync(Pak01.GetHeroAbilities(hero.InternalName));
            foreach (var ability in heroAbilityFile.Where(x => x.Name is not "Version"))
            {
                if (!ability.Children.Any())
                {
                    // TODO have preset filter, log any that aren't caught.  
                    logger.LogInformation("No children for {hero} ability: {ability}", hero.InternalName, ability.Name);
                    continue;
                }

                try
                {
                    heroAbilities.Add(abilityConverter.ConvertUnitAbility(ability));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed parsing hero ability: {ability}", ability.Name);
                }
            }
        }
        // END TEST


        // TODO process

        return [];
    }
}
