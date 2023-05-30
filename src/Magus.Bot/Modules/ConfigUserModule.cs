using Discord;
using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.Extensions;
using Magus.Common.Enums;
using Magus.Data;
using Magus.Data.Extensions;
using Microsoft.Extensions.Options;
using ReverseMarkdown;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System.Net.Http;

namespace Magus.Bot.Modules
{
    [Group(GroupName, "Configure User settings")]
    [ModuleRegistration(Location.TESTING)]
    public class ConfigUserModule : ModuleBase
    {
        const string GroupName = "config-user";

        private readonly ILogger<ConfigUserModule> _logger;
        private readonly IAsyncDataService _db;
        private readonly BotSettings _botSettings;

        public ConfigUserModule(ILogger<ConfigUserModule> logger, IAsyncDataService db, IOptions<BotSettings> botSettings)
        {
            _logger = logger;
            _db = db;
            _botSettings = botSettings.Value;
        }

        const string deleteDataCommand = "delete-data";
        [SlashCommand(deleteDataCommand, "Remove all your data from Magus")]
        public async Task RequestDeleteData()
        {
            await DeferAsync(true);

            var message = new StringBuilder()
                    .AppendLine("This command will remove all saved user configuration, including your linked Steam account.")
                    .AppendLine()
                    .AppendLine("MagusBot only stores data you have provided such as Steam ID, and functional data available as per [Discord's Bot Data Access](https://support.discord.com/hc/en-us/articles/7933951485975).")
                    .AppendLine()
                    .AppendLine("If you are sure you want to delete all your data, please click the button below. This will immediately and irreversibly remove all of your data from MagusBot.");
            var embed = new EmbedBuilder()
                    .WithTitle("Are you sure you want to delete your data from MagusBot?")
                    .WithDescription(message.ToString())
                    .WithFields()
                    .WithColor(Color.Red)
                    .Build();
            var magusButton = new ButtonBuilder()
                    .WithLabel("CONFIRM DELETE")
                    .WithCustomId(deleteDataCommand, GroupName, method: CustomIdMethod.DEL)
                    .WithEmote(new Emoji("⚠️"))
                    .WithStyle(ButtonStyle.Danger);
            var components = new ComponentBuilder()
                    .WithButton(magusButton)
                    .Build();

            await FollowupAsync(embed: embed, components: components, ephemeral: true);
        }

        [ComponentInteraction(deleteDataCommand + ":DEL:")]
        public async Task ConfirmDeleteData()
        {
            await DeferAsync();

            var success = await _db.DeleteUser(Context.User);

            if (success)
                await FollowupAsync("All your data has been deleted. Thank you for using MagusBot.", ephemeral: true);
            else
                await FollowupAsync("Failed to delete data, please try again later.", ephemeral: true);

        }

        [Group(SubGroupName, "user steam settings")]
        public class SteamGroup : InteractionModuleBase<SocketInteractionContext>
        {
            const string SubGroupName = "steam";

            private readonly ILogger<SteamGroup> _logger;
            private readonly IAsyncDataService _db;
            private readonly BotSettings _botSettings;
            private readonly HttpClient _httpClient;
            private readonly SteamWebInterfaceFactory webInterfaceFactory;

            public SteamGroup(ILogger<SteamGroup> logger, IAsyncDataService db, IOptions<BotSettings> botSettings, HttpClient httpClient)
            {
                _logger = logger;
                _db = db;
                _botSettings = botSettings.Value;
                _httpClient = httpClient;

                webInterfaceFactory = new(_botSettings.Steam.SteamKey);
            }

            [SlashCommand("set", "Set your Steam account")]
            public async Task SetSteamId([Summary(description: "steamID, either")] string steamid)
            {
                await DeferAsync(true);

                var steamInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(_httpClient);
                var id = (await steamInterface.ResolveVanityUrlAsync("ahollowedhunter")).Data;
                _logger.LogInformation((await steamInterface.GetPlayerSummaryAsync(id)).Data.Nickname);
                var user = await _db.GetUser(Context.User);

                //await FollowupAsync(text: "SteamID updated", ephemeral: true);
            }
        }
    }
}
