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
    private static readonly string[] HeroInvalidKeys = ["Version", "npc_dota_hero_target_dummy", "npc_dota_hero_base"];
    private static readonly string[] AbilityInvalidKeys = ["Version", "dota_base_ability", "dota_empty_ability", "default_attack"];

    private static readonly Func<KVOPair, bool> HeroFilter = pair
        => !HeroInvalidKeys.Any(x => x.Equals(pair.Key, StringComparison.InvariantCultureIgnoreCase));

    private static readonly Func<KVOPair, bool> AbilityFilter = pair
        => !AbilityInvalidKeys.Any(x => x.Equals(pair.Key, StringComparison.InvariantCultureIgnoreCase));

    private static readonly Func<KVOPair, bool> NotVersion = pair => pair.Key is not "Version";

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
        Localizations localizations;

        using var gameFileProvider = gameFileProviderFactory.Create();
        localizations = await new LocalizationsBuilder(gameFileProvider)
            .WithAbilities(StringUtils.CleanSimple) // TODO should 'clean' here or later when formatting values? 
            .WithDota(StringUtils.CleanSimple)
            .WithHeroLore(StringUtils.CleanSimple)
            .BuildAsync()
            .ConfigureAwait(false);

        var kvSerializerOptions = new KVSerializerOptions { HasEscapeSequences = true };
        var kvAbilityIds        = await gameFileProvider.GetPak01KVFileAsync(Pak01.NpcAbilityIds, kvSerializerOptions);
        var kvAbilities         = await gameFileProvider.GetPak01KVFileAsync(Pak01.NpcAbilities, kvSerializerOptions);
        var kvItems             = await gameFileProvider.GetPak01KVFileAsync(Pak01.Items, kvSerializerOptions);
        var kvHeroes            = await gameFileProvider.GetPak01KVFileAsync(Pak01.NpcHeroes, kvSerializerOptions);

        var baseAbility      = kvAbilities.GetSingleValue(InternalName.AbilityBase);
        var baseHero         = kvHeroes.GetSingleValue(InternalName.HeroBase);
        var itemConverter    = new ItemConverter(baseAbility, ConvertAbilityIds(kvAbilityIds, "ItemAbilities"));
        var abilityConverter = new UnitAbilityConverter(baseAbility, ConvertAbilityIds(kvAbilityIds, "UnitAbilities"));
        var heroConverter    = new HeroConverter(baseHero);

        var items         = ConvertEntities(kvItems.Root.Where(NotVersion), itemConverter);
        var unitAbilities = ConvertEntities(kvAbilities.Root.Where(AbilityFilter), abilityConverter);
        var heroes        = ConvertEntities(kvHeroes.Root.Where(HeroFilter), heroConverter);
        foreach (var heroName in heroes.Keys)
        {
            var heroAbilityFile = await gameFileProvider.GetPak01KVFileAsync(Pak01.GetHeroAbilities(heroName));
            unitAbilities.AddRange(ConvertEntities(heroAbilityFile.Root.Where(NotVersion), abilityConverter));
        }

        // TODO processc

        return [];
    }

    private Dictionary<string, TEntity> ConvertEntities<TEntity>(IEnumerable<KVOPair> entities, IKVObjectConverter<TEntity> converter)
    {
        Dictionary<string, TEntity> converted = new(StringComparer.InvariantCultureIgnoreCase);
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

    private Dictionary<string, int> ConvertAbilityIds(KVObject kvAbilityIds, string groupKey)
    {
        Dictionary<string, int> abilityIds = new(StringComparer.InvariantCultureIgnoreCase);
        foreach ((string name, KVObject ability) in kvAbilityIds[groupKey]["Locked"])
        {
            var abilityId = ability.ToInt32(CultureInfo.InvariantCulture);
            if (!abilityIds.TryAdd(name, abilityId))
                logger.LogWarning("Possible duplicate ability_id for {name}, tried adding {newId} alongside {existingId}", name, abilityId, abilityIds[name]);
        }

        return abilityIds;
    }
}
