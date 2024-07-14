using Discord;
using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.Extensions;
using Magus.Bot.Services;
using Magus.Data;
using Magus.Data.Extensions;
using Microsoft.Extensions.Options;
using SteamWebAPI2.Exceptions;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System.Text.RegularExpressions;

namespace Magus.Bot.Modules;

[Group(GroupName, "Configure User settings")]
[ModuleRegistration(Location.GLOBAL)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
public class ConfigUserModule : ModuleBase
{
    public const string GroupName = "config-user";

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
        public const string SubGroupName = "steam";

        private readonly ILogger<SteamGroup> _logger;
        private readonly IAsyncDataService _db;
        private readonly BotSettings _botSettings;
        private readonly HttpClient _httpClient;
        private readonly SteamWebInterfaceFactory webInterfaceFactory;
        private readonly StratzService _stratz;

        public SteamGroup(ILogger<SteamGroup> logger, IAsyncDataService db, IOptions<BotSettings> botSettings, IHttpClientFactory httpClientFactory, StratzService stratz)
        {
            _logger = logger;
            _db = db;
            _botSettings = botSettings.Value;
            _httpClient = httpClientFactory.CreateClient();
            _stratz = stratz;

            webInterfaceFactory = new(_botSettings.Steam.SteamKey);
        }

        [SlashCommand("set", "Set your Steam account")]
        public async Task SetSteam([Summary(description: "Account ID, e.g: '22202', '76561197960287930', or custom URL 'steamcommunity.com/id/gabelogannewell'")] string account)
        {
            await DeferAsync(true);

            var accountID = await ConvertToAccountID(account.Trim());

            if (accountID == null)
            {
                await FollowupAsync(embed: InvalidSteamIDEmbed());
                return;
            }
            else
            {
                var accountInfo = await _stratz.GetAccountInfo(accountID.Value);

                if (accountInfo.Player == null || accountInfo.Player.SteamAccount == null)
                {
                    await FollowupAsync(embed: NoDotaStats());
                    return;
                }

                await UpdateUserDotaId(accountID.Value);

                var embeds = new List<Embed>
                {
                    // tell the user success, and show basic account details to confirm.
                    new EmbedBuilder()
                    .WithTitle("Successfully updated your Steam account")
                    .WithDescription("You can now use commands like /stats!\n\nIf you have linked the wrong account, just run the command again with the correct ID.")
                    .WithColor(Color.Green)
                    .WithThumbnailUrl(accountInfo.Player.SteamAccount.Avatar)
                    .AddField(accountInfo.Player.SteamAccount.Name, accountInfo.Player.SteamAccount.ProfileUri)
                    .Build()
                };
                if (accountInfo.Player.SteamAccount.IsAnonymous)
                {
                    // tell user to expose public match data
                    embeds.Add(new EmbedBuilder()
                        .WithTitle("YOUR DOTA ACCOUNT IS PRIVATE")
                        .WithDescription("**You won't be able to see any match data while private.**\n\n" +
                        "Please open Dota 2 and change your settings to enable \"Expose Public Match Data\".\n\n" +
                        "Then play a game, or log in to [STRATZ](https://stratz.com/settings) and go to \"Settings\" and click \"Check My Status\"\n\n" +
                        "**You will need to wait a few hours to a day for new stats to update.**\n\n" +
                        "You do not need to set your Steam account via this command again, unless you linked the wrong one.")
                        .WithImageUrl("https://i.imgur.com/cBmEY44.png")
                        .WithColor(Color.Red)
                        .Build());
                }

                await FollowupAsync(embeds: embeds.ToArray());
            }
        }

        public static Embed NoDotaStats()
            => new EmbedBuilder()
                    .WithTitle("No Dota info for this account.")
                    .WithDescription("I could not confirm your account via STRATZ, please check your input and try again.\n\n" +
                        "If your account is new or you only just started playing Dota please play at least one match, wait a few hours, and try again.")
                    .WithColor(Color.LightOrange)
                    .Build();

        private static Embed InvalidSteamIDEmbed()
            => new EmbedBuilder()
                    .WithTitle("Invalid account ID given")
                    .WithDescription("I could not validate this ID. Please check your input and try again.\n\n" +
                    "If you need help getting your ID, you can copy your \"Friend ID\" from your Dota profile.\n\n" +
                    "Or you can use your Friend Code from Steam → Friends → Add Friend page [HERE](https://steamcommunity.com/friends/add/)\n\n" +
                    "Alternatively, you can copy the link to your Steam, STRATZ, OpenDota, or Dotabuff webpage.\n\n" +
                    "Possible valid ID/link examples:\n" +
                    "- 22202\n- 76561197960287930\n- steamcommunity.com/id/gabelogannewell\n- steamcommunity.com/profiles/76561197960287930\n- stratz.com/players/22202\n- www.opendota.com/players/22202")
                    .WithColor(Color.LightOrange)
                    .WithImageUrl("https://i.imgur.com/GZ987LZ.gif")
                    .Build();

        private async Task UpdateUserDotaId(long dotaId)
        {
            var user = await _db.GetUser(Context.User);
            user.DotaID = dotaId;
            await _db.UpsertRecord(user);
        }

        /// <summary>
        /// Convert an input into a Steam Account ID (aka Dota ID)
        /// </summary>
        /// <returns>Parsed Account ID, or null if couldn't detect and convert.</returns>
        private async Task<long?> ConvertToAccountID(string account)
        {
            // It's important to capture the numeric IDs first, as a custom URL may also be numeric only.
            // STEAMID64 first, as it will ALWAYS be 17 characters (on public universe),
            // followed by ACCOUNTID to stop it matching partial STEAMID64.
            // Last group will attempt to capture any valid word-like string, that is valid for steam vanity url.
            // For future ref, also have (?<STEAMID2>STEAM_[10]:[10]:[0-9]+) and (?<STEAMID3>\\[U:[10]:[0-9]+\\])
            var steamIdRegex = new Regex("(?<STEAMID64>(?<![A-Za-z_0-9])[0-9]{17}(?![A-Za-z_0-9]))|(?<ACCOUNTID>(?<![0-9]|STEAM_.+)[0-9]{1,10}(?![0-9:]))|(?<FULLCUSTOMURL>(?<=https?\\:\\/\\/steamcommunity\\.com\\/id\\/)[A-Za-z_0-9]+)|(?<CUSTOMURL>^[A-Za-z_0-9]+$)");

            var match = steamIdRegex.Match(account);

            // Then we get the first NAMED group after "0" that succeeds.
            switch (match.Groups.Values.FirstOrDefault(x => x.Name != "0" && x.Success)?.Name)
            {
                case "STEAMID64":
                    if (ulong.TryParse(match.Value, out var parsedID))
                        return ConvertSteamID64ToAccountID(parsedID);
                    goto default;
                case "ACCOUNTID":
                    if (long.TryParse(match.Value, out var accountId))
                        return accountId;
                    goto default;
                case "FULLCUSTOMURL":
                case "CUSTOMURL":
                    var steamID64 = await SteamID64FromVanityUrl(match.Value);
                    if (steamID64 != null)
                    {
                        return ConvertSteamID64ToAccountID(steamID64.Value);
                    }
                    goto default;
                default:
                    return null;
            };
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
