using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UltimyrArchives.Updater.Services;

namespace UltimyrArchives.Updater;

internal sealed class Updater
{
    private readonly ILogger<Updater> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly StorageService _storageService;

    public Updater(ILogger<Updater> logger, IServiceProvider serviceProvider, StorageService storageService)
    {
        _logger          = logger;
        _serviceProvider = serviceProvider;
        _storageService  = storageService;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Updating Archives...");
        await _storageService.CleanTempIndexesAsync();

        var patchListUpdater = _serviceProvider.GetRequiredService<PatchListUpdater>();
        var latestPatch      = await patchListUpdater.UpdateAndGetLatest();
        
        // TODO - process entities and patchnotes

        // TODO add conditions to swap? Possibly error percentage?
        await _storageService.SwapAllTempIndexesAsync();

        /* TODO don't purge temp right away, leave to swap if issue?
         * Only clean temp indexes for testing currently
         */
        await _storageService.CleanTempIndexesAsync();

        _logger.LogInformation("Finished Updating.");
    }
}
