using Coravel.Scheduling.Schedule.Interfaces;
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
            _logger    = logger;
            _scheduler = scheduler;
            _stratz    = stratz;
            _db        = db;
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

        private string BracketImagePath = Path.GetTempPath() + "bracketImage.png";

        private async Task UpdateBracket()
        {
            var league = await _stratz.GetLeagueInfo(berlinId);
            var playoffNodes = league.NodeGroups.Single(x => x.Name == "Playoffs");

            var grandFinalNodeId = playoffNodes.Nodes.Single(IsGrandFinalNode).Id;
            var ubNodes = playoffNodes.Nodes.Where(x => x.Id < grandFinalNodeId).ToList();
            var lbNodes = playoffNodes.Nodes.Where(x => x.Id > grandFinalNodeId).ToList();

            Font font = SystemFonts.CreateFont("Arial", 48); // for scaling water mark size is largely ignored.

            //var image = new Image<Rgb24>(1400, 1050);
            var image = Image.Load<Rgba32>(Common.Images.BracketTemplate);

            image.Mutate(x => x.DrawText(grandFinalNodeId.ToString(), font, Color.GhostWhite, new Point(1375, 455)));
            for (var i = 0; i < ubNodes.Count; i++)
            {
                var col = i <= 3 ? 200 : i <= 5 ? 600 : 1000;
                var row = 100 * ((i % 4) + 1);

                image.Mutate(x => x.DrawText(ubNodes[i].Id.ToString(), font, Color.GhostWhite, new Point(col, row)));
            }
            
            for (var i = 0; i < lbNodes.Count; i++)
            {
                var col = i <= 3 ? 100 : i <= 7 ? 200 : i <= 9 ? 400 : i <=11 ? 600 : i == 12 ? 800 : 1000;
                var row = (100 * ((i % 4) + 1)) + 500;

                image.Mutate(x => x.DrawText(lbNodes[i].Id.ToString(), font, Color.GhostWhite, new Point(col, row)));
            }


            await image.SaveAsPngAsync(BracketImagePath);

            _logger.LogInformation("Updated DPC Bracket for: {id}. Final node: {node}", berlinId, grandFinalNodeId);
        }
        public string GetBracketImagePath() => BracketImagePath;

        private static bool IsGrandFinalNode(LeagueNodeType node)
            => node.NodeType == LeagueNodeDefaultGroupEnum.BestOfFive || (node.LosingNodeId == null && node.WinningNodeId == null && node.ScheduledTime != null);
    }
}
