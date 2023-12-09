using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Magus.Bot.Attributes;
using Magus.Bot.Extensions;
using Magus.Common.Emotes;
using Magus.Common.Enums;
using Magus.Data.Extensions;
using Magus.Data.Services;
using Microsoft.Extensions.Options;
using SteamWebAPI2.Utilities;

namespace Magus.Bot.Modules;

[DefaultMemberPermissions(GuildPermission.Administrator)]
[EnabledInDm(false)]
[Group(GroupName, "Configure server features")]
[ModuleRegistration(Location.GLOBAL)]
public class ConfigServerModule : ModuleBase
{
    const string GroupName = "config-server";

    private readonly ILogger<ConfigServerModule> _logger;
    private readonly IAsyncDataService _db;
    private readonly BotSettings _botSettings;

    public ConfigServerModule(ILogger<ConfigServerModule> logger, IAsyncDataService db, IOptions<BotSettings> botSettings)
    {
        _logger = logger;
        _db = db;
        _botSettings = botSettings.Value;
    }

    [SlashCommand("announcements", "Configure what announcements to subscribe to")]
    public async Task Announcements()
    {
        var message = new StringBuilder()
                .AppendLine("Select a category below to choose a targetChannel to receive the corresponding updates in.")
                .AppendLine("You can also choose to remove the announcement category from the server.");
        var notes = new StringBuilder()
                .AppendLine("• Each category can only be subscribed to a single targetChannel.")
                .AppendLine("• MagusBot requires the `Manage Webhooks` permission to subscribe to announcements");
        var notesField = new EmbedFieldBuilder()
                .WithName("Please note:")
                .WithValue(notes.ToString());

        var embed = new EmbedBuilder()
                .WithTitle("Configure subscribed announcements")
                .WithDescription(message.ToString())
                .WithFields(notesField)
                .WithColor(Color.Green)
                .Build();

        var dotaButton = new ButtonBuilder()
                .WithLabel(Topic.Dota.ToString())
                .WithCustomId("announcements", GroupName, method: CustomIdMethod.GET, key: Topic.Dota.ToString())
                .WithEmote(MagusEmotes.DotaLogo)
                .WithStyle(ButtonStyle.Primary);
        var magusButton = new ButtonBuilder()
                .WithLabel(Topic.MagusBot.ToString())
                .WithCustomId("announcements", GroupName, method: CustomIdMethod.GET, key: Topic.MagusBot.ToString())
                .WithEmote(MagusEmotes.MagusIcon)
                .WithStyle(ButtonStyle.Primary);
        var rows = new List<ActionRowBuilder>
            {
                new ActionRowBuilder()
                    .WithButton(dotaButton)
                    .WithButton(magusButton)
            };
        var components = new ComponentBuilder()
                .WithRows(rows);

        await RespondAsync(embed: embed, components: components.Build(), ephemeral: true);
    }

    [ComponentInteraction("announcements:GET:*")]
    public async Task AnnouncementsGet(Topic topic)
    {
        var announcement           = (await _db.GetGuild(Context.Guild)).Announcements.FirstOrDefault(x => x.Topic == topic);
        RestWebhook? webhook       = null;
        SocketTextChannel? channel = null;

        if (announcement != null)
        {
            webhook = await Context.Guild.GetWebhookAsync(announcement.WebhookId);
            channel = Context.Guild.GetTextChannel(webhook?.ChannelId ?? 0); // TODO CHECK DEFAULT AFTER NULL CHANGE
        }
        var message = new StringBuilder()
                .AppendFormat("Choose a targetChannel to receive **{0}** related news and updates.", topic)
                .AppendLine();
        if (webhook != null && channel != null)
            message
            .AppendLine()
            .AppendFormat("Currently receiving **{0}** announcements in **{1} {2}**", topic, MagusEmotes.TextChannel.ToString(), channel.Name);

        var fieldValue = new StringBuilder()
                .AppendFormat("• You may only choose **one** {0} Text Channel.", MagusEmotes.TextChannel.ToString())
                .AppendLine()
                .AppendLine("• MagusBot requires the `Manage Webhooks` permission is enabled for the server or selected targetChannel to create or remove webhooks required to send messages.")
                .AppendLine("• Selecting a targetChannel from the menu will **instantly** attempt to update the chosen targetChannel.")
                .AppendLine("• If the process fails, please wait a minute and try again.");

        var embed = new EmbedBuilder()
                .WithTitle($"Configure {topic} announcements")
                .WithDescription(message.ToString())
                .WithColor(Color.Green)
                .WithFields(new EmbedFieldBuilder().WithName("Notes").WithValue(fieldValue.ToString()))
                .Build();

        var dotaUpdatesMenu = new SelectMenuBuilder()
                .WithCustomId("announcements", GroupName, method: CustomIdMethod.SET, key: topic.ToString())
                .WithPlaceholder($"{topic} Updates")
                .WithMaxValues(1)
                .WithMinValues(1)
                .WithType(ComponentType.ChannelSelect)
                .WithChannelTypes(ChannelType.Text);

        var clearButton = new ButtonBuilder()
                .WithLabel("Remove Updates")
                .WithCustomId("announcements", GroupName, method: CustomIdMethod.DEL, key: topic.ToString())
                .WithStyle(ButtonStyle.Danger)
                .WithEmote(new Emoji("⚠️"));

        var components = new ComponentBuilder()
                .WithSelectMenu(dotaUpdatesMenu, row: 0)
                .WithButton(clearButton, 1)
                .Build();

        await RespondAsync(embed: embed, components: components, ephemeral: true);
    }

    [ComponentInteraction("announcements:SET:*")]
    public async Task AnnouncementsSet(Topic topic, SocketTextChannel[] channels)
    {
        await DeferAsync();

        ulong sourceId    = topic == Topic.Dota ? _botSettings.Announcements.DotaSource : _botSettings.Announcements.MagusSource;
        var sourceChannel = Context.Client.GetChannel(sourceId) as INewsChannel;
        var targetChannel = channels[0];
        var guild         = await _db.GetGuild(Context.Guild);

        var name = $"{topic} Announcements";
        if (topic == Topic.Dota)
            name += " via MagusBot";

        var announcement = guild.Announcements.FirstOrDefault(x => x.Topic == topic);
        ulong webhookId;
        RestWebhook? webhook = null;
        if (announcement != null)
        {
            webhookId = announcement.WebhookId;
            webhook = await Context.Guild.GetWebhookAsync(webhookId);
        }
        if (webhook == null)
        {
            webhookId = await sourceChannel!.FollowAnnouncementChannelAsync(targetChannel.Id, null);
            webhook = await Context.Guild.GetWebhookAsync(webhookId);
            if (announcement != null)
            {
                guild.Announcements.Remove(announcement);
            }
            guild.Announcements.Add(new(topic, webhookId));
            await _db.UpsertRecord(guild);
        }
        if (webhook != null)
        {
            await webhook.ModifyAsync(w =>
            {
                w.Name = name;
                w.Channel = targetChannel;
            });
        }

        await FollowupAsync($"**{MagusEmotes.TextChannel}\u00A0{targetChannel.Name}** will now receive {topic} updates!", ephemeral: true);
    }

    [ComponentInteraction("announcements:DEL:*")]
    public async Task AnnouncementsDelete(Topic topic)
    {
        await DeferAsync();

        var guild = await _db.GetGuild(Context.Guild);
        var announcement = guild.Announcements.FirstOrDefault(x => x.Topic == topic);
        if (announcement == null)
        {
            await FollowupAsync($"Not currently subscribed to **{topic}** announcements.", ephemeral: true);
        }
        else
        {
            var webhook = await Context.Guild.GetWebhookAsync(announcement.WebhookId);
            try
            {
                if (webhook != null)
                    await webhook.DeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception trying to delete webhook {webhookId} that appears to have been already removed. Continue to remove from database", announcement.WebhookId);
            }
            guild.Announcements.Remove(announcement);
            await _db.UpsertRecord(guild);

            await FollowupAsync($"Removed **{topic}** announcements from this server.", ephemeral: true);
        }
    }

    [Group(SubGroupName, "server DPC settings")]
    public class DPCGroup : InteractionModuleBase<SocketInteractionContext>
    {
        public const string SubGroupName = "dpc";

        private readonly ILogger<DPCGroup> _logger;
        private readonly IAsyncDataService _db;

        public DPCGroup(ILogger<DPCGroup> logger, IAsyncDataService db)
        {
            _logger = logger;
            _db = db;
        }

        [SlashCommand("spoilers", "Configure whether to hide recent DPC results behind spoiler tags. Defaults to \"On\" to hide results.")]
        public async Task Spoilers([Summary(description: "'On' will hide results behind spoiler tags. 'Off' will show all results.")] SpoilerMode hideSpoilers)
        {
            await DeferAsync(ephemeral: true);

            var guild = await _db.GetGuild(Context.Guild);
            guild.HideDpcSpoilers = hideSpoilers == SpoilerMode.On;
            guild.HasBeenToldOfSpoilers = true;
            await _db.UpsertRecord(guild);

            await FollowupAsync($"Now {(guild.HideDpcSpoilers ? "hiding" : "showing")} spoilers!");
        }
    }

    public enum SpoilerMode
    {
        On = 0,
        Off = 1,
    }
}
