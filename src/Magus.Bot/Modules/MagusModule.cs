﻿using Discord;
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
    private readonly BotSettings _config;

    private readonly MeilisearchService _meilisearchService;

    readonly string version = Assembly.GetEntryAssembly()!.GetName().Version!.ToString(3);

    public MagusModule(IOptions<BotSettings> config, MeilisearchService meilisearchService)
    {
        _config = config.Value;
        _meilisearchService = meilisearchService;
    }

    [SlashCommand("about", "What is MagusBot?")]
    public async Task About()
    {
        await DeferAsync();
        var latestPatch = await _meilisearchService.GetLatestPatchAsync();
        var latestPatchValue = $"[{latestPatch.PatchNumber}](https://www.dota2.com/patches/{latestPatch.PatchNumber}) - <t:{latestPatch.Timestamp}:R>";
        var response = new EmbedBuilder()
        {
            Title = "MagusBot",
            Description = "A DotA 2 focused Discord bot, providing distinct patch notes and information regarding DotA 2 heroes, abilities, and items.\n"
                          + "New and improved features are constantly in development.",
            Color = Color.Purple,
            Footer = new() { Text = "Hot Damn!", IconUrl = Context.Client.CurrentUser.GetAvatarUrl() },
        };
        response.AddField("Version", version, true);
        response.AddField("Latest Patch", latestPatchValue, true);
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
}
