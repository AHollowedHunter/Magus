using Magus.Data.Models.Dota;
using Microsoft.Extensions.Logging;
using UltimyrArchives.Updater.DotaFilePaths;
using UltimyrArchives.Updater.Utils;

namespace UltimyrArchives.Updater;

internal sealed class PatchListProcessor(ILogger<PatchListProcessor> logger, GameFileProviderFactory gameFileProviderFactory)
{
    public async Task<IReadOnlyList<Patch>> GetProcessedAsync()
    {
        logger.LogInformation("Begin Processing Patch List.");

        var patchList = await GetPatchList();

        logger.LogInformation("Finished Processing Patch List.");

        return patchList.AsReadOnly();
    }

    private Task<List<Patch>> GetPatchList()
    {
        using var gameFileProvider = gameFileProviderFactory.Create();

        var patchManifest = gameFileProvider.GetPak01KVFile(Pak01.PatchNotes);

        return Task.FromResult<List<Patch>>([..patchManifest.Root.Select(CreatePatchInfo)]);
    }

    private static Patch CreatePatchInfo(KVOPair pair)
        => new(DotaUtils.GetPatchNumber(pair.Value), DotaUtils.GetPatchTimestamp(pair.Value));
}
