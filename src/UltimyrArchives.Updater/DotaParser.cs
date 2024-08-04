using Microsoft.Extensions.Logging;

namespace UltimyrArchives.Updater;

internal sealed class DotaParser
{
    private readonly ILogger<DotaParser> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DotaParser(ILogger<DotaParser> logger, IServiceProvider serviceProvider)
    {
        _logger          = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task RunParser()
    {
        _logger.LogInformation("Starting Parsing");

        _logger.LogInformation("Finished Parsing");
    }
}
