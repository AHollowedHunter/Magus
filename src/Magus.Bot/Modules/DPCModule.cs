using Discord;
using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Services;
using Magus.Common.Emotes;
using Magus.Data;
using Magus.Data.Extensions;
using Magus.Data.Models.Embeds;
using Magus.Data.Models.Stratz.Results;
using Magus.Data.Models.Stratz.Types;
using Microsoft.Extensions.Options;
using MongoDB.Driver.Linq;
using SixLabors.ImageSharp;
using System.Text.RegularExpressions;

namespace Magus.Bot.Modules
{
    [Group(GroupName, "Get DPC info")]
    [ModuleRegistration(Location.TESTING)]
    public class DPCModule : InteractionModuleBase<SocketInteractionContext>
    {
        const string GroupName = "dpc";

        const string WideSpace = "\u2007\u2007\u2007";

        private readonly ILogger<DPCModule> _logger;
        private readonly BotSettings _config;
        private readonly IAsyncDataService _db;
        private readonly DPCService _dpc;
        private readonly StratzService _stratz;
        private readonly LocalisationService _localisationService;

        public DPCModule(ILogger<DPCModule> logger, IOptions<BotSettings> config, IAsyncDataService db, DPCService dpc, StratzService stratz, LocalisationService localisationService)
        {
            _logger              = logger;
            _db                  = db;
            _config              = config.Value;
            _dpc                 = dpc;
            _stratz              = stratz;
            _localisationService = localisationService;
        }

        [SlashCommand("bracket", "Get the current event bracket standings. Updates every 5 minutes.")]
        public async Task Bracket()
        {
            await DeferAsync();

            using var imageStream = new MemoryStream();
            _dpc.GetBracketImage().SaveAsPng(imageStream);

            await FollowupWithFileAsync(imageStream, "Bracket.png", embed: new EmbedBuilder().WithTitle("Bracket").WithDescription("Upcoming spoiler: ||test vs spoiler||").Build());
        }
    }
}
