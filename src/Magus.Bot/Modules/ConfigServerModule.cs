using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Magus.Bot.Attributes;
using Magus.Bot.Extensions;
using Magus.Common.Enums;
using Magus.Data;
using Magus.Data.Extensions;
using Microsoft.Extensions.Options;

namespace Magus.Bot.Modules
{
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [EnabledInDm(false)]
    [Group(GroupName, "Configure MAGUS features")]
    [ModuleRegistration(Location.GLOBAL)]
    public class ConfigServerModule : ModuleBase
    {
        const string GroupName = "config-server";


        const string SubGroupName = "server";

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
                    .AppendLine("Select a category below to choose a targetChannel to receive the corrosponding updates in.")
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
                    .WithCustomId("announcements", GroupName, SubGroupName, CustomIdMethod.GET, Topic.Dota.ToString())
                    .WithEmote(Emotes.DotaLogo)
                    .WithStyle(ButtonStyle.Primary);
            var magusButton = new ButtonBuilder()
                    .WithLabel(Topic.MagusBot.ToString())
                    .WithCustomId("announcements", GroupName, SubGroupName, CustomIdMethod.GET, Topic.MagusBot.ToString())
                    .WithEmote(Emotes.MagusIcon)
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
                channel = Context.Guild.GetTextChannel(webhook.ChannelId);
            }
            var message = new StringBuilder()
                    .AppendFormat("Choose a targetChannel to recieve **{0}** related news and updates.", topic)
                    .AppendLine();
            if (webhook != null && channel != null)
                message
                .AppendLine()
                .AppendFormat("Currently receiving **{0}** announcments in **{1} {2}**", topic, Emotes.TextChannel.ToString(), channel.Name);

            var fieldValue = new StringBuilder()
                    .AppendFormat("• You may only choose **one** {0} Text Channel.", Emotes.TextChannel.ToString())
                    .AppendLine()
                    .AppendLine("• MagusBot requies the `Manage Webhooks` permission is enabled for the server or selected targetChannel to create or remove webhooks required to send messages.")
                    .AppendLine("• Selecting a targetChannel from the menu will **instantly** attempt to update the chosen targetChannel.")
                    .AppendLine("• If the process fails, please wait a minute and try again.");

            var embed = new EmbedBuilder()
                    .WithTitle($"Configure {topic} announcements")
                    .WithDescription(message.ToString())
                    .WithColor(Color.Green)
                    .WithFields(new EmbedFieldBuilder().WithName("Notes").WithValue(fieldValue.ToString()))
                    .Build();

            var dotaUpdatesMenu = new SelectMenuBuilder()
                    .WithCustomId("announcements", GroupName, SubGroupName, CustomIdMethod.SET, topic.ToString())
                    .WithPlaceholder($"{topic} Updates")
                    .WithMaxValues(1)
                    .WithMinValues(1)
                    .WithType(ComponentType.ChannelSelect)
                    .WithChannelTypes(ChannelType.Text);

            var clearButton = new ButtonBuilder()
                    .WithLabel("Remove Updates")
                    .WithCustomId("announcements", GroupName, SubGroupName, CustomIdMethod.DEL, topic.ToString())
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

            var name = $"{topic} Announcments";
            if (topic == Topic.Dota)
                name += " via MagusBot";

            var announcement = guild.Announcements.FirstOrDefault(x => x.Topic == topic);
            ulong webhookId;
            if (announcement != null)
            {
                webhookId = announcement.WebhookId;
            }
            else
            {
                webhookId = await sourceChannel!.FollowAnnouncementChannelAsync(targetChannel.Id, null);
                guild.Announcements.Add(new(topic, webhookId));
                await _db.UpsertRecord(guild);
            }

            var webhook = await Context.Guild.GetWebhookAsync(webhookId);
            if (webhook != null)
            {
                await webhook.ModifyAsync(w =>
                {
                    w.Name = name;
                    w.Channel = targetChannel;
                });
            }

            await FollowupAsync($"**{Emotes.TextChannel}\u00A0{targetChannel.Name}** will now receive {topic} updates!", ephemeral: true);
        }

        [ComponentInteraction("announcements:DEL:*")]
        public async Task AnnouncementsDelete(Topic topic)
        {
            await DeferAsync();

            var guild = await _db.GetGuild(Context.Guild);
            RestWebhook webhook;
            var announcement = guild.Announcements.FirstOrDefault(x => x.Topic == topic);
            if (announcement == null)
            {
                await FollowupAsync($"Not currently subscribed to **{topic}** announcements.", ephemeral: true);
            }
            else
            {
                webhook = await Context.Guild.GetWebhookAsync(announcement.WebhookId);
                if (webhook != null)
                    await webhook.DeleteAsync();
                guild.Announcements.Remove(announcement);
                await _db.UpsertRecord(guild);

                await FollowupAsync($"Removed **{topic}** announcements from this server.", ephemeral: true);
            }
        }

    }
}
