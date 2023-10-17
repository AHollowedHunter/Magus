using Discord;
using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.Services;
using Magus.Data;
using Magus.Data.Extensions;
using Microsoft.Extensions.Options;
using STRATZ;
using System.Text.RegularExpressions;

namespace Magus.Bot.Modules
{
    [Group(GroupName, "Get DPC info (BETA)")]
    [ModuleRegistration(Location.GLOBAL)]
    public class DPCModule : InteractionModuleBase<SocketInteractionContext>
    {
        const string GroupName = "ti";

        const string WideSpace = "\u2007\u2007\u2007";

        private readonly ILogger<DPCModule> _logger;
        private readonly BotSettings _config;
        private readonly IAsyncDataService _db;
        private readonly DPCService _dpc;
        private readonly StratzService _stratz;
        private readonly LocalisationService _localisationService;

        public DPCModule(ILogger<DPCModule> logger, IOptions<BotSettings> config, IAsyncDataService db, DPCService dpc, StratzService stratz, LocalisationService localisationService)
        {
            _logger = logger;
            _db = db;
            _config = config.Value;
            _dpc = dpc;
            _stratz = stratz;
            _localisationService = localisationService;
        }

        [SlashCommand("bracket", "Get the current event bracket standings. Updates every 5 minutes. (BETA)")]
        public async Task Bracket()
        {
            await DeferAsync();

            var getGuildTask =_db.GetGuild(Context.Guild);

            var bracketInfo = _dpc.BracketInfo;
            using var imageStream = new MemoryStream();
            bracketInfo.GetBracketPng(imageStream);

            var guild = await getGuildTask;
            var spoilerMode = guild?.HideDpcSpoilers ?? false;

            var embeds = new List<Embed>
            {
                MakeBracketEmbed(bracketInfo, spoilerMode)
            };
            if (guild is not null && guild.HasBeenToldOfSpoilers is false)
            {
                embeds.Add(SpoilerModeWarning());
                guild.HasBeenToldOfSpoilers = true;
                // No need to wait for this and delay response. If it fails the guild will get another notification
                _ = _db.UpsertRecord(guild);
            }

            await FollowupWithFileAsync(imageStream, MakeFileName(spoilerMode, bracketInfo), embeds: embeds.ToArray());
        }

        private static Embed MakeBracketEmbed(LeagueInfo info, bool spoilerMode)
        {
            var eb = new EmbedBuilder()
                .WithTitle(info.LeagueName)
                .WithDescription($"[View on STRATZ](https://stratz.com/leagues/{info.LeagueId}){WideSpace}[Official Website]({info.Url})")
                .WithColor(0x102a4c)
                .WithTimestamp(info.LastUpdated)
                .WithFooter("Powered by STRATZ", "https://cdn.discordapp.com/emojis/1113573151549423657.webp");

            if (info.Playoffs.LiveNodes.Any())
            {
                eb.AddField("_ _", Emotes.Live + " **Live Games**");
                foreach (var node in info.Playoffs.LiveNodes)
                {
                    eb.AddField(MakeNodeFieldBuilder(node, spoilerMode));
                }
            }
            if (info.Playoffs.UpcomingNodes.Any())
            {
                eb.AddField("_ _", "**Upcoming Games**");
                foreach (var node in info.Playoffs.UpcomingNodes.Take(3))
                {
                    eb.AddField(MakeNodeFieldBuilder(node, spoilerMode).WithIsInline(true));
                }
            }

            return eb.Build();
        }

        private static EmbedFieldBuilder MakeNodeFieldBuilder(LeagueNodeType node, bool spoilerMode)
        {
            string? format(string? text) => text is not null && spoilerMode ? Format.Spoiler(text) : text;

            var name = new StringBuilder()
                        .Append(format(node.TeamOne?.Name.PadRight(8, '\u2002')) ?? "*TBD*")
                        .Append(" vs ")
                        .Append(format(node.TeamTwo?.Name.PadRight(8, '\u2002')) ?? "*TBD*")
                        .ToString();
            var value = new StringBuilder()
                        .Append(node.HasStarted ?? false ? "Started" : "Starts")
                        .Append(" <t:")
                        .Append(node.ActualTime ?? node.ScheduledTime)
                        .AppendLine(":R>");
            if (node.HasStarted ?? false)
            {
                value.Append("Score: ")
                    .Append(format(node.TeamOneWins + " - " + node.TeamTwoWins));
            }

            return new EmbedFieldBuilder() { Name = name, Value = value };
        }

        private static string MakeFileName(bool spoilerMode, LeagueInfo info)
            => new StringBuilder()
            .Append(spoilerMode ? "SPOILER_" : string.Empty)
            .Append("Bracket_")
            .Append(info.LeagueId)
            .Append('_')
            .Append(info.LastUpdated.ToUnixTimeSeconds())
            .Append(".png")
            .ToString();

        private static Embed SpoilerModeWarning()
            => new EmbedBuilder()
            .WithTitle("Spoiler Mode is turned on")
            .WithDescription("In servers spoiler mode is switched on by default for results, live games, and main events. A server admin can use `/config-server dpc spoilers` to disable this. In DMs spoiler mode is off.")
            .WithColor(Color.DarkGreen)
            .Build();

        [SlashCommand("upcoming", "Get upcoming games. Updates every 5 minutes. (BETA)")]
        public async Task Upcoming()
        {
            await DeferAsync();

            var getGuildTask =_db.GetGuild(Context.Guild);

            var info = _dpc.BracketInfo;

            var embed = new EmbedBuilder()
                .WithTitle(info.LeagueName)
                .WithDescription($"[View on STRATZ](https://stratz.com/leagues/{info.LeagueId}){WideSpace}[Official Website]({info.Url})\n**Upcoming Matches**")
                .WithColor(0x102a4c)
                .WithTimestamp(info.LastUpdated)
                .WithFooter("Powered by STRATZ", "https://cdn.discordapp.com/emojis/1113573151549423657.webp");

            var guild = await getGuildTask;
            var spoilerMode = guild?.HideDpcSpoilers ?? false;

            foreach (var node in info.AllUpcomingNodes.Take(4))
            {
                // Only hide spoilers for main event playoffs, which for bali is NOW 13.
                // long term this should be covered with a proper set of structs etc. for this data.
                embed.AddField(MakeNodeFieldBuilder(node, node.NodeGroupId == 13 ? spoilerMode : false));
            }
            var embeds = new List<Embed>
            {
                embed.Build()
            };
            if (guild is not null && guild.HasBeenToldOfSpoilers is false)
            {
                embeds.Add(SpoilerModeWarning());
                guild.HasBeenToldOfSpoilers = true;
                // No need to wait for this and delay response. If it fails the guild will get another notification
                _ = _db.UpsertRecord(guild);
            }

            await FollowupAsync(embeds: embeds.ToArray());
        }

        [SlashCommand("live", "Get live games. Updates every 5 minutes. (BETA)")]
        public async Task Live()
        {
            await DeferAsync();

            var getGuildTask =_db.GetGuild(Context.Guild);

            var info = _dpc.BracketInfo;

            var mainEmbed = new EmbedBuilder()
                .WithTitle(info.LeagueName)
                .WithDescription($"[View on STRATZ](https://stratz.com/leagues/{info.LeagueId}){WideSpace}[Official Website]({info.Url})\n{Emotes.Live} **Live Games**")
                .WithColor(0x102a4c)
                .WithTimestamp(info.LastUpdated)
                .WithFooter("Powered by STRATZ", "https://cdn.discordapp.com/emojis/1113573151549423657.webp");

            var guild = await getGuildTask;
            var spoilerMode = guild?.HideDpcSpoilers ?? false;

            if (info.LiveMatches.Any())
            {
                foreach (var match in info.LiveMatches.Where(x => x.GameState is not MatchLiveGameState.PostGame))
                {
                    mainEmbed.AddField(MakeLiveGameFieldBuilder(match, spoilerMode));
                }
            }
            else
            {
                mainEmbed.AddField("No Live Games Right Now", "Use `/dpc upcoming` to see when the next series is starting.");
            }
            var embeds = new List<Embed>
            {
                mainEmbed.Build()
            };
            if (guild is not null && guild.HasBeenToldOfSpoilers is false)
            {
                embeds.Add(SpoilerModeWarning());
                guild.HasBeenToldOfSpoilers = true;
                // No need to wait for this and delay response. If it fails the guild will get another notification
                _ = _db.UpsertRecord(guild);
            }

            await FollowupAsync(embeds: embeds.ToArray());
        }
        private static EmbedFieldBuilder MakeLiveGameFieldBuilder(MatchLiveType match, bool spoilerMode)
        {
            string? format(string? text) => text is not null && spoilerMode ? Format.Spoiler(text) : text;

            var name = new StringBuilder().Append(format(match.RadiantTeam?.Name.PadRight(8, '\u2002')) ?? "*UNKNOWN*")
                                          .Append(" vs ")
                                          .Append(format(match.DireTeam?.Name.PadRight(8, '\u2002')) ?? "*UNKNOWN*")
                                          .ToString();
            var duration = TimeSpan.FromSeconds(match.GameTime ?? 0).ToString("h\\:mm\\:ss");
            var value = new StringBuilder().Append("Started <t:")
                                           .Append(match.CreatedDateTime)
                                           .Append(":R>")
                                           .Append(WideSpace)
                                           .Append("State: ")
                                           .AppendLine(match.GameState.ToString())
                                           .Append("Game Time: ")
                                           .AppendLine(duration)
                                           .Append("Score: ")
                                           .Append(format(match.RadiantScore + " - " + match.DireScore));


            return new EmbedFieldBuilder() { Name = name, Value = value };
        }
    }
}
