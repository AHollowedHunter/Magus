using AngleSharp.Dom;
using Coravel.Scheduling.Schedule.Interfaces;
using GraphQLParser.AST;
using Magus.Data;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using STRATZ;

namespace Magus.Bot.Services
{
    public class DPCService
    {
        private readonly ILogger<StratzService> _logger;
        private readonly IScheduler _scheduler;
        private readonly StratzService _stratz;
        private readonly IAsyncDataService _db;

        public DPCService(ILogger<StratzService> logger, IScheduler scheduler, StratzService stratz, IAsyncDataService db)
        {
            _logger = logger;
            _scheduler = scheduler;
            _stratz = stratz;
            _db = db;
        }


        public async Task InitialiseAsync()
        {
            ScheduleUpdateBracket();

            _logger.LogInformation("DPCService Initalised");
        }

        private void ScheduleUpdateBracket() => _scheduler.ScheduleAsync(UpdateBracket)
                                                          .EveryThirtySeconds()
                                                          .RunOnceAtStart();

        const int berlinId = 15251;
        const int baliId   = 15438;
        const int limaId   = 15089;
        const int ti_22    = 14268;

        private string BracketImagePath = Path.GetTempPath() + "bracketImage.png";

        private async Task UpdateBracket()
        {
            var league = await _stratz.GetLeagueInfo(berlinId);
            var playoffNodes = league.NodeGroups.Single(x => x.Name.StartsWith("Playoff"));

            var gfNode = playoffNodes.Nodes.Single(IsGrandFinalNode);
            var ubNodes = playoffNodes.Nodes.Where(x => x.Id < gfNode.Id).ToList();
            var lbNodes = playoffNodes.Nodes.Where(x => x.Id > gfNode.Id).ToList();

            var image = Image.Load<Rgba32>(Common.Images.BracketTemplate);

            var font = SystemFonts.CreateFont("Noto Sans", 12); // temporary measure, should include any fonts in resources.

            // temp bracket "anchors", the top-left point of each bracket card of template at 1600x1000
            var gfAnchor = (x: 1375, y:455);
            var ubAnchors = new (int x, int y)[] { (250, 60), (250, 170), (250, 280), (250, 390), (700, 115), (700, 335), (1150, 225) };
            var lbAnchors = new (int x, int y)[] { (25, 520), (25, 630), (25, 740), (25, 850), (250, 520), (250, 630), (250, 740), (250, 850), (475, 575), (475, 795), (700, 575), (700, 795), (925, 685), (1150, 685) };

            AddNodeToImage(image, gfNode, gfAnchor);

            for (var i = 0; i < ubNodes.Count; i++)
            {
                AddNodeToImage(image, ubNodes[i], ubAnchors[i]);
            }
            // assuming the "round 1" nodes are included. change this to cope in case they are not
            for (var i = 0; i < lbNodes.Count; i++)
            {
                AddNodeToImage(image, lbNodes[i], lbAnchors[i]);
            }
            //

            // Cache image in temp folder for now
            // todo check and improve this
            await image.SaveAsPngAsync(BracketImagePath);

            _logger.LogInformation("Updated DPC Bracket for: {id}. Final node: {node}", berlinId, gfNode.Id);
        }

        private static void AddNodeToImage(Image image, LeagueNodeType node, (int x, int y) anchor)
        {
            var font = SystemFonts.CreateFont("Noto Sans", 12); // temporary measure, should include any fonts in resources.
            var textOptions = new TextOptions(font) { Dpi = 96, WrappingLength = 160 };

            textOptions.Origin = new Point(4 + anchor.x, 24 + anchor.y);
            if (node.TeamOne != null)
            {
                image.Mutate(x => x.DrawText(textOptions, node.TeamOne.Name, Color.GhostWhite));
                textOptions.Origin = new Point(184 + anchor.x, 24 + anchor.y);
                image.Mutate(x => x.DrawText(textOptions, node.TeamOneWins.ToString(), Color.GhostWhite));
            }

            textOptions.Origin = new Point(4 + anchor.x, 56 + anchor.y);
            if (node.TeamTwo != null)
            {
                image.Mutate(x => x.DrawText(textOptions, node.TeamTwo.Name, Color.GhostWhite));
                textOptions.Origin = new Point(184 + anchor.x, 56 + anchor.y);
                image.Mutate(x => x.DrawText(textOptions, node.TeamTwoWins.ToString(), Color.GhostWhite));
            }
        }


        public string GetBracketImagePath() => BracketImagePath;

        private static bool IsGrandFinalNode(LeagueNodeType node)
            => node.NodeType == LeagueNodeDefaultGroupEnum.BestOfFive || (node.LosingNodeId == null && node.WinningNodeId == null && node.ScheduledTime != null);
    }
}
