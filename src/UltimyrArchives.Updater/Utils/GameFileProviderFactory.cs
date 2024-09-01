using Microsoft.Extensions.Options;

namespace UltimyrArchives.Updater.Utils;

internal sealed class GameFileProviderFactory(IOptions<UpdaterConfig> config)
{
    private readonly string _rootGamePath = config.Value.GameFileLocation;

    public GameFileProvider Create() => new(_rootGamePath);
}
