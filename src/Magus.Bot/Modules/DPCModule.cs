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
            var spoilerMode = guild.HideDpcSpoilers;

            var embeds = new List<Embed>
            {
                MakeBracketEmbed(bracketInfo, spoilerMode)
            };
            if (!guild.HasBeenToldOfSpoilers)
            {
                embeds.Add(SpoilerModeWarning());
                guild.HasBeenToldOfSpoilers = true;
                // No need to wait for this and delay response. If it fails the guild will get another notification
                _ = _db.UpsertRecord(guild);
            }

            await FollowupWithFileAsync(imageStream, MakeFileName(spoilerMode, bracketInfo), embeds: embeds.ToArray());
        }

        private static Embed MakeBracketEmbed(LeagueBracketInfo info, bool spoilerMode)
        {
            var eb = new EmbedBuilder()
                .WithTitle(info.LeagueName)
                .WithDescription($"[View on STRATZ](https://stratz.com/leagues/{info.LeagueId}){WideSpace}[Official Website]({info.Url})")
                .WithColor(0x102a4c)
                .WithTimestamp(info.LastUpdated)
                .WithFooter("Powered by STRATZ", "https://cdn.discordapp.com/emojis/1113573151549423657.webp");

            if (info.LiveNodes.Any())
            {
                eb.AddField("_ _", Emotes.Live + " **Live Games**");
                foreach (var node in info.LiveNodes)
                {
                    eb.AddField(MakeNodeFieldBuilder(node, spoilerMode));
                }
            }
            if (info.UpcomingNodes.Any())
            {
                eb.AddField("_ _", "**Upcoming Games**");
                foreach (var node in info.UpcomingNodes.Take(3))
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

        private static string MakeFileName(bool spoilerMode, LeagueBracketInfo info)
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
            .WithDescription("By default, spoiler mode is switched on for ongoing and recent events. A server admin can use `/config-server dpc spoilers` to disable this.")
            .WithColor(Color.DarkGreen)
            .Build();

        [SlashCommand("upcoming", "Get upcoming games. Updates every 5 minutes. (BETA)")]
        public async Task Upcoming()
        {
            await DeferAsync();

            var upcoming = _dpc.UpcomingNodes;

            var embed = new EmbedBuilder()
                .WithTitle("UPCOMING MATCHES")
                .WithDescription($"<t:{upcoming.First().ScheduledTime}:f>");

            foreach (var node in upcoming)
            {
                embed.AddField(UpcomingNodeField(node));
            }

            await FollowupAsync(embed: embed.Build());
        }

        private static EmbedFieldBuilder UpcomingNodeField(LeagueNodeType node)
        {
            var name = new StringBuilder()
                .Append(node.TeamOne.Name)
                .Append(" vs ")
                .Append(node.TeamTwo.Name)
                .Append(" (")
                .Append(node.NodeGroupId)
                .Append(')')
                .ToString();
            var value = "_ _";
            return new EmbedFieldBuilder()
                .WithName(name)
                .WithValue(value);
        }
    }
}
