using Discord;
using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Common.Enums;
using Magus.Data.Extensions;
using Magus.Data.Models.Discord;
using Magus.Data.Services;
using Microsoft.Extensions.Options;

namespace Magus.Bot.Modules;

[DefaultMemberPermissions(GuildPermission.Administrator)]
[RequireOwner]
[Group(GroupName, "Only for the owner")]
[ModuleRegistration(Location.MANAGEMENT)]
public class ManagementModule : ModuleBase
{
    const string GroupName = "manage-magus";

    private readonly ILogger<ManagementModule> _logger;
    private readonly IAsyncDataService _db;
    private readonly BotSettings _botSettings;

    public ManagementModule(ILogger<ManagementModule> logger, IAsyncDataService db, IOptions<BotSettings> botSettings)
    {
        _logger = logger;
        _db = db;
        _botSettings = botSettings.Value;
    }

    [SlashCommand("set-status", "playing, watching; whatever")]
    public async Task SetStatus(string status, [Summary(description: "Can't be CustomStatus")] ActivityType type, [Summary(description: "If streaming, the url")] string? url = null)
    {
        if (type is ActivityType.CustomStatus)
            await Context.Client.SetCustomStatusAsync(status);
        else
            await Context.Client.SetGameAsync(name: status, streamUrl: url, type: type);
        await RespondAsync("Done", ephemeral: true);
    }

    [Group(SubGroupName, "get info of things")]
    public class InfoGroup : InteractionModuleBase<SocketInteractionContext>
    {
        const string SubGroupName = "info";

        private readonly ILogger<InfoGroup> _logger;
        private readonly IAsyncDataService _db;
        private readonly BotSettings _botSettings;

        public InfoGroup(ILogger<InfoGroup> logger, IAsyncDataService db, IOptions<BotSettings> botSettings)
        {
            _logger = logger;
            _db = db;
            _botSettings = botSettings.Value;
        }

        [SlashCommand("announcements", "total subscriptions")]
        public async Task Announcements()
        {
            await DeferAsync(ephemeral: true);
            var embed = new EmbedBuilder()
                .WithTitle("Total guilds using announcements")
                .AddField("Dota", await _db.GetTotalAnnouncementSubscriptions(Topic.Dota), true)
                .AddField("Magus", await _db.GetTotalAnnouncementSubscriptions(Topic.MagusBot), true)
                .AddField("Total Both", await _db.GetTotalAnnouncementSubscriptions(), true)
                .Build();
            await FollowupAsync(embed: embed, ephemeral: true);
        }

        [SlashCommand("guilds", "guild stats")]
        public async Task Guilds()
        {
            await DeferAsync(ephemeral: true);
            var guilds       = await _db.GetRecords<Guild>();
            var currentCount = guilds.Where(g => g.IsCurrentMember).Count();
            var leftCount    = guilds.Where(g => !g.IsCurrentMember).Count();
            var retention    = Math.Round((decimal)currentCount / guilds.Count() * 100, 2);
            var retentionEmote = retention switch
            {
                0      => "💀",
                < 20   => "😱",
                < 50   => "😢",
                < 80   => "😟",
                <= 100 => "😄",
                _      => "🤔"
            };
            var embed = new EmbedBuilder()
                .WithTitle("Guilds info")
                .AddField("Total Current", $"{currentCount} ({Context.Client.Guilds.Count})", true)
                .AddField("Retention", $"{retention}% - {retentionEmote}" , true)
                .AddField("Total Left", guilds.Where(g => !g.IsCurrentMember).Count(), true)
                .AddField("Total Community", guilds.Where(g => g.IsCommunity).Count(), true)
                .AddField("Total Discoverable", guilds.Where(g => g.IsDiscoverable).Count(), true)
                .AddField("Total Featureable", guilds.Where(g => g.IsFeatureable).Count(), true)
                .AddField("Total Partnered", guilds.Where(g => g.IsPartnered).Count(), true)
                .AddField("Total Verified", guilds.Where(g => g.IsVerified).Count(), true)
                .Build();
            await FollowupAsync(embed: embed, ephemeral: true);
        }
    }

    [Group(SubGroupName, "updates things")]
    public class UpdateGroup : InteractionModuleBase<SocketInteractionContext>
    {
        const string SubGroupName = "update";

        private readonly ILogger<UpdateGroup> _logger;
        private readonly IAsyncDataService _db;
        private readonly BotSettings _botSettings;

        public UpdateGroup(ILogger<UpdateGroup> logger, IAsyncDataService db, IOptions<BotSettings> botSettings)
        {
            _logger = logger;
            _db = db;
            _botSettings = botSettings.Value;
        }

        [SlashCommand("guilds", "Update Guilds in DB")]
        public async Task Guilds()
        {
            await RespondAsync("Updating Guilds... Please stand-by.", ephemeral: true);
            _logger.LogInformation("Manually updating Guilds in DB");
            foreach (var guild in Context.Client.Guilds)
            {
                try
                {
                    await _db.UpsertGuildRecord(guild);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception attempting to update guild {id}", guild.Id);
                }
            }
            await ModifyOriginalResponseAsync(x => x.Content = "Finished updating guilds. Check logs for any errors");
        }
    }

    [Group(SubGroupName, "commands and such")]
    public class ModulesGroup : InteractionModuleBase<SocketInteractionContext>
    {
        const string SubGroupName = "modules";

        private readonly ILogger<InfoGroup> _logger;
        private readonly IAsyncDataService _db;
        private readonly BotSettings _botSettings;
        private readonly InteractionHandler _interactionHandler;
        private readonly InteractionService _interactionService;

        public ModulesGroup(ILogger<InfoGroup> logger, IAsyncDataService db, IOptions<BotSettings> botSettings, InteractionHandler interactionHandler, InteractionService interactionService)
        {
            _logger = logger;
            _db = db;
            _botSettings = botSettings.Value;
            _interactionHandler = interactionHandler;
            _interactionService = interactionService;
        }

        [SlashCommand("re-register", "Re-register all commands, like when starting bot")]
        public async Task ReRegister()
        {
            await DeferAsync(ephemeral: true);
            await _interactionHandler.RegisterModulesAsync();
            await FollowupAsync("Finished", ephemeral: true);
        }

        [SlashCommand("remove-guild", "remove all guild commands from a guild")]
        public async Task RemoveGuild(string guildId)
        {
            await DeferAsync(ephemeral: true);
            if (!ulong.TryParse(guildId, out var parsedId))
            {
                await FollowupAsync("That ain't a `ulong`");
            }
            else if (_botSettings.ManagementGuilds.Contains(parsedId))
            {
                await FollowupAsync("Cannot remove a management guilds commands. Do it manually!");
            }
            else if (!Context.Client.Guilds.Any(g => g.Id == parsedId))
            {
                await FollowupAsync($"Bot is not in any guilds with id `{guildId}`! Check and try again.");
            }
            else
            {
                try
                {
                    await _interactionService.AddModulesToGuildAsync(parsedId, true);
                    await FollowupAsync($"Removed commands from guild `{guildId}`");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove commands from guild: {guildId}", guildId);
                    await FollowupAsync("Failed to remove commands. Check logs.");
                }
            }
        }

#if DEBUG
        [SlashCommand("dev-re-register", "Re-register to dev. DEBUG ONLY")]
        public async Task ReRegisterDevGuild()
        {
            await DeferAsync(ephemeral: true);
            await _interactionService.RegisterCommandsToGuildAsync(_botSettings.DevGuild, true);
            await FollowupAsync("Done");
        }

        [SlashCommand("remove-global", "REMOVE ALL GLOBAL COMMANDS. DEBUG ONLY")]
        public async Task RemoveGlobal()
        {
            await DeferAsync(ephemeral: true);
            await _interactionService.AddModulesGloballyAsync(true);
            await FollowupAsync("⚠️ REMOVED ALL GLOBAL MODULES ⚠️");
        }
#endif
    }
}

