﻿using Magus.Common;
using Magus.Data;

namespace Magus.DataBuilder
{
    public class DotaUpdater
    {
        private readonly IAsyncDataService _db;
        private readonly ILogger<PatchNoteUpdater> _logger;
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _services;

        public DotaUpdater(IAsyncDataService db, ILogger<PatchNoteUpdater> logger, HttpClient httpClient, IServiceProvider services)
        {
            _db         = db;
            _logger     = logger;
            _httpClient = httpClient;
            _services   = services;
        }

        public async Task Update(DotaInfo dotaInfo = DotaInfo.ALL)
        {
            await UpdatePatchList(); // Always run this, update to know most recent patch

            if (dotaInfo.HasFlag(DotaInfo.ENTITIES))
                await UpdateEntities();

            if (dotaInfo.HasFlag(DotaInfo.PATCHNOTES))
                await UpdatePatchNotes();
        }

        private async Task UpdatePatchList()
        {
            var patchListUpdater = new PatchListUpdater(_db, _logger, _httpClient);
            await patchListUpdater.Update();
        }

        private async Task UpdateEntities()
        {
            using var scope = _services.CreateScope();
            var entityUpdate = scope.ServiceProvider.GetRequiredService<EntityUpdater>();
            await entityUpdate.Update();
        }

        private async Task UpdatePatchNotes()
        {
            using var scope = _services.CreateScope();
            var entityUpdate = scope.ServiceProvider.GetRequiredService<PatchNoteUpdater>();
            await entityUpdate.Update();
        }

        [Flags]
        public enum DotaInfo
        {
            PATCHLIST  = 0x0,
            ENTITIES   = 0x1,
            PATCHNOTES = 0x2,
            ALL        = 0x3,
        }
    }
}
