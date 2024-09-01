using Magus.Data.Models.Dota;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UltimyrArchives.Updater.Converters;
using UltimyrArchives.Updater.DotaFilePaths;
using UltimyrArchives.Updater.Utils;

namespace UltimyrArchives.Updater;

internal sealed class PatchNotesProcessor(ILogger<PatchNotesProcessor> logger, GameFileProviderFactory gameFileProviderFactory)
{
    public async Task<IReadOnlyList<PatchNote>> GetProcessedAsync(Entity[] entities)
    {
        logger.LogInformation("Processing Patch Notes.");
        var stopwatch = Stopwatch.StartNew();

        var patchNotes = await GetPatchNotesAsync(entities);

        stopwatch.Stop();
        logger.LogInformation("Finished Processing Patch Notes, took {timeTaken}.", stopwatch.Elapsed);

        return patchNotes;
    }

    private async Task<PatchNote[]> GetPatchNotesAsync(Entity[] entities)
    {
        LocalisedValues localisedValues;
        KVDocument      patchManifest;
        using (var gameFileProvider = gameFileProviderFactory.Create())
        {
            localisedValues = await new LocalisedValuesBuilder(gameFileProvider)
                .WithPatchNotes(StringUtils.CleanPatchNote)
                .BuildAsync()
                .ConfigureAwait(false);
            patchManifest = await gameFileProvider.GetPak01KVFileAsync(Pak01.PatchNotes).ConfigureAwait(false);
        }

        var patchNoteConverter = new PatchNoteConverter();
        var manifests          = patchManifest.Select(patchNoteConverter.Convert).ToArray();

        // TODO process

        return [];
    }
}
