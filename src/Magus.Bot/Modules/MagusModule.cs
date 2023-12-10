using Discord;
using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.AutocompleteHandlers;
using Magus.Common.Emotes;
using Magus.Data.Models.V2;
using Magus.Data.Services;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Magus.Bot.Modules;

[Group("magus", "All things MagusBot")]
[ModuleRegistration(Location.GLOBAL)]
public class MagusModule : ModuleBase
{
    private readonly IAsyncDataService _db;
    private readonly BotSettings _config;

    private readonly MeilisearchService _meilisearchService = new(); // HACK

    readonly string version = Assembly.GetEntryAssembly()!.GetName().Version!.ToString(3);

    public MagusModule(IAsyncDataService db, IOptions<BotSettings> config)
    {
        _db = db;
        _config = config.Value;
    }

    [SlashCommand("about", "What is MagusBot?")]
    public async Task About()
    {
        await DeferAsync();
        var latestPatchNote = await _db.GetLatestPatch();
        var latestPatch = $"[{latestPatchNote.PatchNumber}](https://www.dota2.com/patches/{latestPatchNote.PatchNumber}) - <t:{latestPatchNote.Timestamp}:R>";
        var response = new EmbedBuilder()
        {
            Title = "MagusBot",
            Description = "A DotA 2 focused Discord bot, providing distinct patch notes and information regarding DotA 2 heroes, abilities, and items.\n"
                          + "New and improved features are constantly in development.",
            Color = Color.Purple,
            Footer = new() { Text = "Hot Damn!", IconUrl = Context.Client.CurrentUser.GetAvatarUrl() },
        };
        response.AddField("Version", version, true);
        response.AddField("Latest Patch", latestPatch, true);
        response.AddField("Total Guilds", Context.Client.Guilds.Count, true);
        response.AddField("Acknowledgements", "SteamDB for various libraries\nDiscord.NET library", false);

        var links = $"[Bot Invite Link]({_config.BotInvite})\n[Discord Server]({_config.BotServer})\n[MagusBot.xyz](https://magusbot.xyz)\n[Privacy Policy]({_config.BotPrivacyPolicy})\n[Terms of Service]({_config.BotTermsOfService})\n";
        response.AddField("Links", links, false);
        response.AddField(MagusEmotes.Spacer.ToString(), "Dota and the Dota Logo are trademarks and/or registered trademarks of Valve Corporation");

        await FollowupAsync(embed: response.Build(), ephemeral: true);
    }

    [SlashCommand("invite", "Where shall I go next? Ultimyr University? Yama Raskav? Hmm.")]
    public async Task Invite()
    {
        await DeferAsync();
        await FollowupAsync(text: "Share me with your friends (or server admin) with my invite link!\n" + _config.BotInvite);
    }

    [SlashCommand("privacy", "Privacy Policy for MagusBot")]
    public async Task Privacy()
    {
        await DeferAsync();
        await FollowupAsync(text: "To view the privacy policy for MagusBot, please follow the link below:\n" + _config.BotPrivacyPolicy);
    }

    [SlashCommand("terms", "Terms of Service for MagusBot")]
    public async Task Term()
    {
        await DeferAsync();
        await FollowupAsync(text: "To view the terms of service for MagusBot, please follow the link below:\n" + _config.BotTermsOfService);
    }

    [SlashCommand("test", "testing")] // TEST
    public async Task Test([Autocomplete(typeof(HeroAutocompleteHandler))] string query)
    {
        await DeferAsync().ConfigureAwait(false);

        var result = await _meilisearchService.SearchTopResultAsync<EntityMeta>(query);

        var embed = new EmbedBuilder()
            .WithTitle("Test")
            .WithDescription("No footer text but image only")
            .WithFooter("    ", "https://cdn.discordapp.com/emojis/1098946942802862160.webp")
            .WithTimestamp(DateTimeOffset.Now);

        await FollowupAsync(text: result?.InternalName ?? "Nothing", embed: embed.Build());
    }
}
