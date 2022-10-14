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
            response.AddField(new EmbedFieldBuilder() { Name = "Version", Value = version, IsInline = true });
            response.AddField(new EmbedFieldBuilder() { Name = "Latest Patch", Value = latestPatch, IsInline = true });
            response.AddField(new EmbedFieldBuilder() { Name = "Total Guilds", Value = Context.Client.Guilds.Count(), IsInline = true });
            response.AddField(new EmbedFieldBuilder() { Name = "Acknowledgements", Value = "SteamDB for various libraries + Gametracking-Dota2\nDiscord.NET library", IsInline = false });
            
            var links = $"[Bot Invite Link]({_config.BotInvite})\n[Discord Server]({_config.BotServer})\n[MagusBot.xyz](https://magusbot.xyz)\n[Privacy Policy]({_config.BotPrivacyPolicy})\n";
            response.AddField(new EmbedFieldBuilder() { Name = "Links", Value = links, IsInline = false });
            response.AddField(new EmbedFieldBuilder() { Name = Emotes.Spacer.ToString(), Value="Dota and the Dota Logo are trademarks and/or registered trademarks of Valve Corporation" });

            await RespondAsync(embed: response.Build(), ephemeral: true);
        }

        [SlashCommand("invite", "Where shall I go next? Ultimyr University? Yama Raskav? Hmm.")]
        public async Task Invite()
        {
            await RespondAsync(text: "Share me with your friends (or server admin) with my invite link!\n" + _config.BotInvite);
        }

        [SlashCommand("privacy", "Privacy Policy for MagusBot")]
        public async Task Privacy()
        {
            await RespondAsync(text: "To view the privacy policy for MagusBot, please follow the link below:\n" + _config.BotPrivacyPolicy);
        }
    }
}
