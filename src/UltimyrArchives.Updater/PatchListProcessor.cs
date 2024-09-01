using Magus.Data.Models.Dota;
using Microsoft.Extensions.Logging;
using UltimyrArchives.Updater.DotaFilePaths;
using UltimyrArchives.Updater.Extensions;
using UltimyrArchives.Updater.Utils;

namespace UltimyrArchives.Updater;

internal sealed class PatchListProcessor(ILogger<PatchListProcessor> logger, GameFileProviderFactory gameFileProviderFactory)
{
    public async Task<IReadOnlyList<Patch>> GetProcessedAsync()
    {
        logger.LogInformation("Begin Processing Patch List.");

        var patchList = await GetPatchList();

        logger.LogInformation("Finished Processing Patch List.");

        return patchList;
    }

    private Task<List<Patch>> GetPatchList()
    {
        using var gameFileProvider = gameFileProviderFactory.Create();

        var patchManifest = gameFileProvider.GetPak01KVFile(Pak01.PatchNotes);

        return Task.FromResult<List<Patch>>([..patchManifest.Children.Select(CreatePatchInfo)]);
    }

    private static Patch CreatePatchInfo(KVObject patch)
    {
        var patchNumber = patch.GetRequiredString("patch_name", CultureInfo.InvariantCulture)[6..];
        return new Patch(patchNumber, PatchUtils.GetPatchTimestamp(patch));
    }
}
