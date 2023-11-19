using Discord;
using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.Services;
using Magus.Data;
using Microsoft.Extensions.Options;

namespace Magus.Bot.Modules;

//[Group("ti", "The International commands")]
[ModuleRegistration(Location.GLOBAL, isEnabled: false)]
public sealed class TIModule : ModuleBase
{
    private readonly IAsyncDataService _db;
    private readonly BotSettings _config;
    private readonly TIService _tiService;

    private static readonly uint TI2022_ID = 14268;

    public TIModule(IAsyncDataService db, IOptions<BotSettings> config, TIService tiService)
    {
        _db = db;
        _config = config.Value;
        _tiService = tiService;
    }

    [SlashCommand("prize-pool", "Get current TI Prize pool.")]
    public async Task PrizePool()
    {
        var embed = new EmbedBuilder()
        {
            Title        = "The International 2022 Prize Pool",
            Description  = $"Current Prize Pool stands at:\n\n**${string.Format("{0:n0}", _tiService.PrizePool)}**",
            Timestamp    = DateTimeOffset.UtcNow,
            Color        = Color.Gold,
            ThumbnailUrl = DotaUrls.DotaColourLogo,
        };
        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("live", "Get live games.")]
    public async Task LiveGames()
    {
        var embed = new EmbedBuilder()
        {
            Title        = "The International 2022 Live Games",
            Timestamp    = DateTimeOffset.UtcNow,
            Color        = Color.Gold,
            ThumbnailUrl = DotaUrls.DotaColourLogo,
        };

        foreach (var game in _tiService.LiveGames)
        {
            var name = $"{game.RadiantTeam?.TeamName ?? "[UNKNOWN]"} vs {game.DireTeam?.TeamName ?? "[UNKNOWN]"}";

            var value = $"Duration:\u2007**{game.Duration:h\\:mm\\:ss}**{Emotes.Spacer}*(Stream Delay:\u2007{game.StreamDelaySeconds.TotalSeconds}s)*\n"
                        + $"Score:\u2007||**{game.Scores.Radiant}\u00A0-\u00A0{game.Scores.Dire}**||{Emotes.Spacer}"
                        + (game.SeriesWins.Radiant > 0 || game.SeriesWins.Dire > 0 ? $"Series Wins:\u2007**||{game.SeriesWins.Radiant}\u00A0-\u00A0{game.SeriesWins.Dire}||**\n" : "\n")
                        + $"Match ID:\u2007{game.MatchId}";
            embed.AddField(name, value);
        }
        if (!_tiService.LiveGames.Any())
            embed.Description = "No live games right now.\nCheck the schedule here: https://www.dota2.com/esports/ti11/schedule";

        await RespondAsync(embed: embed.Build());
    }
}
