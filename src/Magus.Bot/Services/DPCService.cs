﻿using Coravel.Scheduling.Schedule.Interfaces;
using Magus.Common.ImageSharp;
using Magus.Data;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using STRATZ;
using System.Collections.Immutable;

namespace Magus.Bot.Services
{
    public class DPCService
    {
        private readonly ILogger<StratzService> _logger;
        private readonly IScheduler _scheduler;
        private readonly StratzService _stratz;
        private readonly IAsyncDataService _db;
        private readonly HttpClient _httpClient;

        private readonly IDictionary<int, Image> teamLogos = new Dictionary<int, Image>();

        public DPCService(ILogger<StratzService> logger, IScheduler scheduler, StratzService stratz, IAsyncDataService db, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _scheduler = scheduler;
            _stratz = stratz;
            _db = db;
            _httpClient = httpClientFactory.CreateClient();
        }


        public async Task InitialiseAsync()
        {
            // Update team logos first, then run update bracket.
            await UpdateTeamLogos();
            ScheduleUpdateTeamLogos();

            ScheduleUpdateBracket();
            ScheduleUpdateUpcoming();

            _logger.LogInformation("DPCService Initialised");
        }

        private void ScheduleUpdateTeamLogos() => _scheduler.ScheduleAsync(UpdateTeamLogos)
                                                            .DailyAtHour(3);

        private async Task UpdateTeamLogos()
        {
            var league = await _stratz.GetLeagueInfo(berlinId);

            foreach (var team in league.Tables.TableTeams)
            {
                if (team.TeamId != null)
                {
                    try
                    {
                        var teamLogo = Image.Load<Rgba32>(await _httpClient.GetStreamAsync(DotaUrls.GetTeamLogo((int)team.TeamId)));
                        teamLogo.Mutate(x => x.Resize(32, 32));
                        teamLogos[(int)team.TeamId] = teamLogo;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to update team logo for {id}", team.TeamId);
                    }
                }
            }
            _logger.LogInformation("Updated Team Logos");
        }

        private void ScheduleUpdateUpcoming() => _scheduler.ScheduleAsync(UpdateUpcoming)
                                                           .EveryFifteenSeconds()
                                                           .RunOnceAtStart();

        private IEnumerable<LeagueNodeType> upcomingNodes;

        public IEnumerable<LeagueNodeType> UpcomingNodes => upcomingNodes.ToArray();

        private async Task UpdateUpcoming()
        {
            var league = await _stratz.GetLeagueInfo(baliId);

            var allNodes = league.NodeGroups.SelectMany(x => x.Nodes).ToList();

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var allUpcoming = allNodes.Where(x => !x.HasStarted ?? false && x.ScheduledTime >= now);
            upcomingNodes = allNodes.Where(x => x.ScheduledTime == allNodes.Where(x => x.ScheduledTime >= now).MinBy(x => x.ScheduledTime)?.ScheduledTime);
        }

        private void ScheduleUpdateBracket() => _scheduler.ScheduleAsync(UpdateBracket)
                                                          .EveryFifteenSeconds()
                                                          .RunOnceAtStart();

        const int berlinId = 15251;
        const int baliId   = 15438;
        const int limaId   = 15089;
        const int ti_22    = 14268;

        private LeagueBracketInfo _bracketInfo;

        public LeagueBracketInfo BracketInfo => _bracketInfo;

        private bool IsPlayoffNodeGroup(LeagueNodeGroupType x)
            => x.NodeGroupType == LeagueNodeGroupTypeEnum.BracketDoubleSeedLoser || x.NodeGroupType == LeagueNodeGroupTypeEnum.BracketDoubleAllWinner;

        private async Task UpdateBracket()
        {
            var league = await _stratz.GetLeagueInfo(baliId);
            var playoffNodeGroup = league.NodeGroups.Single(IsPlayoffNodeGroup);

            var bracketImage = Image.Load<Rgba32>(Common.Images.BracketTemplateBali);

            PopulateBracketImage(bracketImage, playoffNodeGroup);

            var liveNodes = GetLiveNodes(playoffNodeGroup);
            var upcomingNodes = GetUpcomingNodes(playoffNodeGroup);

            _bracketInfo = new LeagueBracketInfo((int)league.Id!, league.DisplayName, league.TournamentUrl, league.PrizePool, DateTimeOffset.UtcNow, bracketImage);
            _logger.LogInformation("Updated DPC Bracket");
        }

        private void PopulateBracketImage(Image bracketImage, LeagueNodeGroupType playoffNodeGroup)
        {
            // Grand final node seems to be at the end of the UB node ID series.
            // Similarly, the LB nodes come AFTER the GF nodeId.
            // If this changes in the future, need to use an algorithm to process
            // node order via the winning/losing node IDs
            var gfNode = playoffNodeGroup.Nodes.FirstOrDefault(IsGrandFinalNode);
            if (gfNode == null)
            {
                // If this happens, but other nodes are already populated, then will need to rework
                // how everything is calculated using winning/losing node IDs
                // BUT 🤞 as node ids are in a particular order, they should all be generated at once.
                _logger.LogWarning("Cannot determine Grand Final node.");
            }
            else
            {
                var ubNodes = playoffNodeGroup.Nodes.Where(x => x.Id < gfNode.Id).ToList();
                var lbNodes = playoffNodeGroup.Nodes.Where(x => x.Id > gfNode.Id).ToList();

                // Some leagues have it set as SeedLoser but "bye" the LB teams through round 1.
                // Can use this to split template to ignore the extra round and save space.
                var skipSeedRound = playoffNodeGroup.NodeGroupType == LeagueNodeGroupTypeEnum.BracketDoubleAllWinner || playoffNodeGroup.TeamCount <= 12;

                // temp bracket "anchors", the top-left point of each bracket card of template at 1600x1000
                var gfAnchorSeed = (x: 1375, y:455);
                var ubAnchorsSeed = new (int x, int y)[] { (250, 60), (250, 170), (250, 280), (250, 390), (700, 115), (700, 335), (1150, 225) };
                var lbAnchorsSeed = new (int x, int y)[] { (25, 520), (25, 630), (25, 740), (25, 850), (250, 520), (250, 630), (250, 740), (250, 850), (475, 575), (475, 795), (700, 575), (700, 795), (925, 685), (1150, 685) };

                var gfAnchorWinners = (x: 1150, y:455);
                var ubAnchorsWinners = new (int x, int y)[] { (25, 60), (25, 170), (25, 280), (25, 390), (475, 115), (475, 335), (925, 225) };
                var lbAnchorsWinners = new (int x, int y)[] { (25, 520), (25, 630), (25, 740), (25, 850), (250, 575), (250, 795), (475, 575), (475, 795), (700, 685), (925, 685) };

                AddNodeToImage(bracketImage, gfNode, gfAnchorWinners);

                for (var i = 0; i < ubNodes.Count; i++)
                {
                    AddNodeToImage(bracketImage, ubNodes[i], ubAnchorsWinners[i]);
                }

                // remove the "ghost seed" rounds
                if (skipSeedRound && lbNodes.Count == 14)
                    lbNodes.RemoveRange(0, 4);
                for (var i = 0; i < lbNodes.Count; i++)
                {
                    AddNodeToImage(bracketImage, lbNodes[i], lbAnchorsWinners[i]);
                }
            }
        }

        private void AddNodeToImage(Image image, LeagueNodeType node, (int x, int y) anchor)
        {
            var timeFont = FontFamilies.NotoSans.CreateFont(10);
            var nameFont = FontFamilies.NotoSans.CreateFont(13);
            var scoreFont = FontFamilies.NotoSans.CreateFont(20, FontStyle.Bold);
            var timeOptions = new TextOptions(timeFont) { Dpi = 96, WrappingLength = 192};
            var nameOptions = new TextOptions(nameFont) { Dpi = 96, LineSpacing = 0.75F, VerticalAlignment = VerticalAlignment.Center, WrappingLength = 140};
            var scoreOptions = new TextOptions(scoreFont) { Dpi = 96, VerticalAlignment = VerticalAlignment.Center };

            var time = node.ActualTime ?? node.ScheduledTime;
            if (time != null)
            {
                var displayTime = DateTimeOffset.FromUnixTimeSeconds((long)time).ToString("ddd dd HH:mm");
                var textSize = TextMeasurer.Measure(displayTime, timeOptions);
                timeOptions.Origin = new Point(anchor.x + 100 - (int)(textSize.Width / 2), anchor.y);
                image.Mutate(x => x.DrawText(timeOptions, displayTime, new Color(new Rgb24(230, 230, 240))));
            }
            if (node.TeamOne != null)
            {
                nameOptions.Origin = new Point(36 + anchor.x, 32 + anchor.y);
                image.Mutate(x => x.DrawText(nameOptions, node.TeamOne.Name, Color.GhostWhite));
                scoreOptions.Origin = new Point(180 + anchor.x, 32 + anchor.y);
                image.Mutate(x => x.DrawText(scoreOptions, node.TeamOneWins.ToString(), Color.GhostWhite));

                if (teamLogos.ContainsKey(node.TeamOne.Id ?? -1))
                    image.Mutate(x => x.DrawImage(teamLogos[node.TeamOne.Id ?? -1], new Point(anchor.x + 2, anchor.y + 18), 1));
            }
            if (node.TeamTwo != null)
            {
                nameOptions.Origin = new Point(36 + anchor.x, 70 + anchor.y);
                image.Mutate(x => x.DrawText(nameOptions, node.TeamTwo.Name, Color.GhostWhite));
                scoreOptions.Origin = new Point(180 + anchor.x, 70 + anchor.y);
                image.Mutate(x => x.DrawText(scoreOptions, node.TeamTwoWins.ToString(), Color.GhostWhite));

                if (teamLogos.ContainsKey(node.TeamTwo.Id ?? -1))
                    image.Mutate(x => x.DrawImage(teamLogos[node.TeamTwo.Id ?? -1], new Point(anchor.x + 2, anchor.y + 56), 1));
            }
        }

        private static bool IsGrandFinalNode(LeagueNodeType node)
            => node.NodeType == LeagueNodeDefaultGroupEnum.BestOfFive || (node.LosingNodeId == null && node.WinningNodeId == null && node.ScheduledTime != null);

        private IEnumerable<LeagueNodeType> GetLiveNodes(LeagueNodeGroupType nodeGroup)
        {
            var currentNodes = nodeGroup.Nodes.Where(x => (x.HasStarted ?? false) && (!x.IsCompleted ?? false)).OrderBy(x => x.ActualTime ?? x.ScheduledTime);
            return currentNodes;
        }
        private IEnumerable<LeagueNodeType> GetUpcomingNodes(LeagueNodeGroupType nodeGroup)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var upcomingNodes = nodeGroup.Nodes.Where(x => !x.HasStarted ?? false && x.ScheduledTime >= now).OrderBy(x => x.ScheduledTime);
            return upcomingNodes;
        }
    }

    public readonly struct LeagueBracketInfo
    {
        public LeagueBracketInfo(int leagueId, string Name, string url, int? prizePool, DateTimeOffset lastUpdated, Image image)
        {
            LeagueId = leagueId;
            LeagueName = Name;
            Url = url;
            PrizePool = prizePool;
            LastUpdated = lastUpdated;
            _bracketImage = image;
        }

        public int LeagueId { get; init; }
        public string LeagueName { get; init; }
        public string Url { get; init; }
        public int? PrizePool { get; init; }
        public DateTimeOffset LastUpdated { get; init; }

        private Image _bracketImage { get; init; }

        public void GetBracketPng(Stream stream) => _bracketImage.SaveAsPng(stream);
    }
}
