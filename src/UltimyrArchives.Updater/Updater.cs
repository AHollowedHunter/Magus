using Microsoft.Extensions.Logging;

namespace UltimyrArchives.Updater;

internal sealed class Updater
{
    private readonly ILogger<Updater> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Updater(ILogger<Updater> logger, IServiceProvider serviceProvider)
    {
        _logger          = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting Parsing");

        _logger.LogInformation("Finished Parsing");
    }
}
