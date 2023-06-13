using Discord;
using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.Extensions;
using Magus.Data;
using Magus.Data.Extensions;
using Microsoft.Extensions.Options;
using SteamWebAPI2.Exceptions;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System.Text.RegularExpressions;

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

            public SteamGroup(ILogger<SteamGroup> logger, IAsyncDataService db, IOptions<BotSettings> botSettings, IHttpClientFactory httpClientFactory)
            {
                _logger = logger;
                _db = db;
                _botSettings = botSettings.Value;
                _httpClient = httpClientFactory.CreateClient();

                webInterfaceFactory = new(_botSettings.Steam.SteamKey);
            }

            [SlashCommand("set", "Set your Steam account")]
            public async Task SetSteam([Summary(description: "Steam account ID, like '22202', '76561197960287930', or custom URL 'gabelogannewell'.")] string account)
            {
                await DeferAsync(true);

                // It's important to capture the numeric IDs first, as a custom URL may also be numeric only.
                // STEAMID64 first, as it will ALWAYS be 17 characters (on public universe),
                // followed by ACCOUNTID to stop it matching partial STEAMID64.
                // Last group will attempt to capture any valid word-like string, that is valid for steam vanity url.
                // For future ref, also have (?<STEAMID2>STEAM_[10]:[10]:[0-9]+) and (?<STEAMID3>\\[U:[10]:[0-9]+\\])
                var steamIdRegex = new Regex("(?<STEAMID64>(?<![A-Za-z_0-9])[0-9]{17}(?![A-Za-z_0-9]))|(?<ACCOUNTID>(?<![0-9]|STEAM_.+)[0-9]{1,10}(?![0-9:]))|(?<FULLCUSTOMURL>(?<=https?\\:\\/\\/steamcommunity\\.com\\/id\\/)[A-Za-z_0-9]+)|(?<CUSTOMURL>^[A-Za-z_0-9]+$)");

                var match = steamIdRegex.Match(account);

                long accountId;
                // Then we get the first NAMED group after "0" that succeeds.
                switch (match.Groups.Values.FirstOrDefault(x => x.Name != "0" && x.Success)?.Name)
                {
                    case "STEAMID64":
                        if (ulong.TryParse(match.Value, out var parsedID))
                        {
                            accountId = ConvertSteamID64ToAccountID(parsedID);
                            await FollowupAsync("STEAMID64: " + accountId);
                        }
                        break;
                    case "ACCOUNTID":
                        if (long.TryParse(match.Value, out accountId))
                        {
                            await FollowupAsync("ACCOUNTID: " + accountId);
                        }
                        break;
                    case "FULLCUSTOMURL":
                    case "CUSTOMURL":
                        var steamID64 = await SteamID64FromVanityUrl(match.Value);
                        if (steamID64 != null)
                        {
                            accountId = ConvertSteamID64ToAccountID(steamID64.Value);
                            await FollowupAsync("VANITY: " + accountId);
                        }
                        break;
                    default:
                        // Tell the user what's accepted
                        await FollowupAsync("WRONG");
                        break;
                };

                // still testing
                var user = await _db.GetUser(Context.User);

                //user.DotaID = accountId;
                //var steamId64 = await SteamID64FromVanityUrl(match.Value);

                //if (steamId64 != null)
                //{
                //    user.DotaID = ConvertSteamID64ToAccountID(76561198063635869);
                //    var steamInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(_httpClient);
                //    _logger.LogInformation((await steamInterface.GetPlayerSummaryAsync(steamId64.Value)).Data.Nickname);
                //}


                //await FollowupAsync(text: "SteamID updated", ephemeral: true);
            }

            /// <see cref="https://developer.valvesoftware.com/wiki/SteamID"/>
            private static long ConvertSteamID64ToAccountID(ulong id64)
             => (long)(((id64 >> 0b_1) & 0b_0111_1111_1111_1111_1111_1111_1111_1111) * 2 + (id64 & 1));

            private async Task<ulong?> SteamID64FromVanityUrl(string url)
            {
                try
                {
                    var steamInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(_httpClient);
                    return (await steamInterface.ResolveVanityUrlAsync(url)).Data;
                }
                catch (VanityUrlNotResolvedException ex)
                {
                    _logger.LogWarning("Failed to resolve a vanity URL", ex);
                    return null;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning("Error reaching Steam, is it Tuesday night?", ex);
                    return null;
                }
            }
        }
    }
}
