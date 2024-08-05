using Magus.Data.Models.Dota;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using UltimyrArchives.Updater.DotaFilePaths;
using UltimyrArchives.Updater.Extensions;
using UltimyrArchives.Updater.Services;
using UltimyrArchives.Updater.Utils;
using ValveKeyValue;

namespace UltimyrArchives.Updater;

internal sealed class PatchListUpdater
{
    private readonly ILogger<PatchListUpdater> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly StorageService _storageService;

    public PatchListUpdater(ILogger<PatchListUpdater> logger, IServiceProvider serviceProvider, StorageService storageService)
    {
        _logger          = logger;
        _serviceProvider = serviceProvider;
        _storageService  = storageService;
    }

    public async Task<Patch> UpdateAndGetLatest()
    {
        _logger.LogInformation("Updating Patch List.");

        var patchList = await GetPatchList();

        await _storageService.StorePatchListTempAsync(patchList);

        _logger.LogInformation("Finished Updating Patch List.");

        return patchList.OrderByDescending(p => p.Timestamp).First();
    }

    private Task<List<Patch>> GetPatchList()
    {
        using var gameFileProvider = _serviceProvider.GetRequiredService<GameFileProvider>();

        var patchManifest = gameFileProvider.GetPak01TextFile(Pak01.PatchNotes);
        
        return Task.FromResult<List<Patch>>([..patchManifest.Children.Select(CreatePatchInfo)]);
    }

    private static Patch CreatePatchInfo(KVObject patch)
    {
        var patchNumber = patch.GetRequiredString("patch_name", CultureInfo.InvariantCulture).Replace("patch", "").Trim();
        return new Patch(patchNumber, PatchUtils.GetPatchTimestamp(patch));
    }
}
