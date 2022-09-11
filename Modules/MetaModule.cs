using Discord;
using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Common;
using Magus.Data;
using System.Reflection;

namespace Magus.Bot.Modules
{
    [Group("magus", "All things MagusBot")]
    [ModuleRegistration(Location.GLOBAL)]
    public class MetaModule : ModuleBase
    {
        private readonly IAsyncDataService _db;
        private readonly Configuration _config;
        private readonly ILogger<MetaModule> _logger;

        readonly string version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";

        public MetaModule(IAsyncDataService db, Configuration config, ILogger<MetaModule> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        [SlashCommand("about", "Face the Magus!")]
        public async Task About()
        {
            var author = await Context.Client.GetUserAsync(240463688627126278);
            var latestPatchNote = await _db.GetLatestPatch();
            var latestPatch = $"[{latestPatchNote.PatchNumber}](https://www.dota2.com/patches/{latestPatchNote.PatchNumber}) - <t:{latestPatchNote.Timestamp}:R>";
            var response = new EmbedBuilder()
            {
                Title = "MagusBot",
                Description = "A DotA 2 Discord bot",
                //Author = new EmbedAuthorBuilder() { Name = "AHollowedHunter", Url = $"https://github.com/AHollowedHunter", IconUrl = "https://avatars.githubusercontent.com/u/45659989?v=4&s=48" },
                Color = Color.Purple,
                Footer = new() { Text = "Hot Damn!", IconUrl = Context.Client.CurrentUser.GetAvatarUrl() },
            };
            response.AddField(new EmbedFieldBuilder() { Name = "Version", Value = version, IsInline = true });
            response.AddField(new EmbedFieldBuilder() { Name = "Latest Patch", Value = latestPatch, IsInline = true });
            response.AddField(new EmbedFieldBuilder() { Name = "Total Guilds", Value = Context.Client.Guilds.Count(), IsInline = false });

            var links = $"[Bot Invite Link]({_config.BotInvite})\n[Discord Server]({_config.BotServer})\n[MagusBot.xyz](https://magusbot.xyz)\n";
            response.AddField(new EmbedFieldBuilder() { Name = "Links", Value = links, IsInline = false });

            await RespondAsync(embed: response.Build(), ephemeral: true);
        }

        [SlashCommand("invite", "Where shall I go next? Ultimyr University? Yama Raskav? Hmm.")]
        public async Task Invite()
        {
            await RespondAsync(text: "Share me with your friends (or server admin) with my invite link!\n" + _config.BotInvite);
        }
    }
}
