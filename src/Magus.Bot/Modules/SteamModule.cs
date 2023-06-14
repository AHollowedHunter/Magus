using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Data;
using Microsoft.Extensions.Options;

namespace Magus.Bot.Modules
{
    [Group(GroupName, "steam commands")]
    [ModuleRegistration(Location.TESTING, isEnabled: false)]
    public class SteamModule : InteractionModuleBase<SocketInteractionContext>
    {
        const string GroupName = "steam";

        private readonly ILogger<SteamModule> _logger;
        private readonly BotSettings _config;
        private readonly IAsyncDataService _db;
        public SteamModule(ILogger<SteamModule> logger, IOptions<BotSettings> config, IAsyncDataService db)
        {
            _logger = logger;
            _db = db;
            _config = config.Value;
        }
    }
}
