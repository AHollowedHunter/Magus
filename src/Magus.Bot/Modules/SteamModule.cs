using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Data;
using Magus.Data.Models.Discord;
using Microsoft.Extensions.Options;

namespace Magus.Bot.Modules
{
    [Group(GroupName, "steam commands")]
    [ModuleRegistration(Location.TESTING)]
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

        [SlashCommand("set-id", "Set your Steam ID")]
        public async Task SetSteamId(uint steamId)
        {
            await DeferAsync(true);

            var user = new User() { DotaID = steamId};

            _logger.LogDebug("ID: {id}", user.DotaID);

            await FollowupAsync(text: "SteamID updated", ephemeral: true);
        }
    }
}
