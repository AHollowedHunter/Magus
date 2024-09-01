using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UltimyrArchives.Updater.Services;

namespace UltimyrArchives.Updater;

internal sealed class Updater
{
    private readonly ILogger<Updater> _logger;
    private readonly StorageService _storageService;
    private readonly PatchListProcessor _patchListProcessor;
    private readonly PatchNotesProcessor _patchNotesProcessor;

    public Updater(ILogger<Updater> logger, StorageService storageService, PatchListProcessor patchListProcessor, PatchNotesProcessor patchNotesProcessor)
    {
        _logger              = logger;
        _storageService      = storageService;
        _patchListProcessor  = patchListProcessor;
        _patchNotesProcessor = patchNotesProcessor;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Updating Archives...");
        var stopwatch = Stopwatch.StartNew();
        await _storageService.CleanTempIndexesAsync();

        var patchList   = await _patchListProcessor.GetProcessedAsync();
        var latestPatch = patchList.MaxBy(p => p.Timestamp);
        await _storageService.StorePatchListTempAsync(patchList);


        // TODO - process entities and patchnotes
        // TODO get Entity and EntityInfo before PatchNotes.

        var patchNotes = await _patchNotesProcessor.GetProcessedAsync([ /* TODO */]);

        // await _storageService.StorePatchNotesTempAsync(patchNotes);

        // TODO add conditions to swap? Possibly error percentage?
        await _storageService.SwapAllTempIndexesAsync();

        /* TODO don't purge temp right away, leave to swap if issue?
         * Only clean temp indexes for testing currently
         */
        await _storageService.CleanTempIndexesAsync();

        stopwatch.Stop();
        _logger.LogInformation("Finished Updating, total time taken {TimeTaken}.", stopwatch.Elapsed);
    }
}
