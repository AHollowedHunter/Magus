using Coravel.Scheduling.Schedule.Interfaces;
using Magus.Common.Options;
using Magus.Data;
using Magus.Data.Models.Magus;
using Microsoft.Extensions.Options;

namespace Magus.Bot.Services
{
    /*
     * This service is filling the need before a rework of localisation etc.
     * 
     * Ideally this service would be able to server the AutocompleteHandlers,
     * but unfortunately re-working those now would be more waste of time.
     */
    public class EntityNameLocalisationService
    {
        private readonly IAsyncDataService _db;
        private readonly ILogger<EntityNameLocalisationService> _logger;
        private readonly IScheduler _scheduler;
        private readonly LocalisationOptions _localisationOptions;

        private IEnumerable<EntityLocalisation> heroLocalisations = new List<EntityLocalisation>();

        public EntityNameLocalisationService(IAsyncDataService db, ILogger<EntityNameLocalisationService> logger, IScheduler scheduler, IOptions<LocalisationOptions> localisationOptions)
        {
            _db                  = db;
            _logger              = logger;
            _scheduler           = scheduler;
            _localisationOptions = localisationOptions.Value;
        }

        public async Task InitialiseAsync()
        {
            ScheduleGetDotaNews();

            _logger.LogInformation("EntityNameLocalisationService Initialised");
        }

        private void ScheduleGetDotaNews() => _scheduler.ScheduleAsync(UpdateFromDatabase)
                                                        .Monthly()
                                                        .RunOnceAtStart();

        private async Task UpdateFromDatabase()
        {
            try
            {
                heroLocalisations = await _db.GetEntityLocalisations(Data.Enums.EntityType.HERO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Couldn't update localisations.");
            }
        }

        public string GetLocalisedHeroName(int heroId, string locale)
            => heroLocalisations.First(hero => hero.EntityId == heroId).GetLocalisedNameOrDefault(locale);
    }
}
