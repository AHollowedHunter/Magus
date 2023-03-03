using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Magus.Bot.Attributes;
using Magus.Bot.Extensions;
using Magus.Common;
using Magus.Common.Enums;
using Magus.Data;
using Magus.Data.Extensions;

namespace Magus.Bot.Modules
{
    [Group(GroupName, "Configure MAGUS features")]
    [ModuleRegistration(Location.TESTING)]
    public class ConfigModule : ModuleBase
    {
        const string GroupName = "config";

        private readonly ILogger<ConfigModule> _logger;
        private readonly IAsyncDataService _db;

        public ConfigModule(ILogger<ConfigModule> logger, IAsyncDataService db)
        {
            _logger = logger;
            _db = db;
        }


        [Group(SubGroupName, "Configure server-specific settings")]
        public class ConfigServerModule : InteractionModuleBase<SocketInteractionContext>
        {
            const string SubGroupName = "server";

            private readonly ILogger<ConfigServerModule> _logger;
            private readonly IAsyncDataService _db;

            public ConfigServerModule(ILogger<ConfigServerModule> logger, IAsyncDataService db)
            {
                _logger = logger;
                _db = db;
            }

            [SlashCommand("announcements", "Configure what announcements to subscribe to")]
            public async Task Announcements()
            {
                var message = new StringBuilder()
                    .AppendLine("Select a category below to choose a channel to receive the corrosponding updates in.")
                    .AppendLine("You can also choose to remove the announcement category from the server.");
                var notes = new StringBuilder()
                    .AppendLine("• Each category can only be subscribed to a single channel.")
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
                var message = new StringBuilder()
                    .AppendFormat("Choose a channel to recieve **{0}** related news and updates.", topic)
                    .AppendLine()
                    .AppendFormat("• You may choose an {0} Topic or {1} Text Channel.", Emotes.AnnouncementChannel.ToString(), Emotes.TextChannel.ToString())
                    .AppendLine()
                    .AppendLine("• MagusBot requies the `Manage Webhooks` permission is enabled for the server or selected channel to create or remove webhooks required to send messages.")
                    .AppendLine("• Selecting a channel from the menu will **instantly** attempt to update the chosen channel.")
                    .AppendLine("• If the process fails, please wait a minute and try again.");

                var embed = new EmbedBuilder()
                    .WithTitle($"Configure {topic} announcements")
                    .WithDescription(message.ToString())
                    .WithColor(Color.Green)
                    .Build();

                var dotaUpdatesMenu = new SelectMenuBuilder()
                    .WithCustomId("announcements", GroupName, SubGroupName, CustomIdMethod.SET, topic.ToString())
                    .WithPlaceholder($"{topic} Updates")
                    .WithMaxValues(1)
                    .WithMinValues(1)
                    .WithType(ComponentType.ChannelSelect)
                    .WithChannelTypes(ChannelType.News, ChannelType.Text);

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
                var channel = channels[0];

                // Do something
                var guild = await _db.GetGuild(Context.Guild);

                RestWebhook webhook;
                var announcement = guild.Announcements.FirstOrDefault(x => x.Topic == topic);
                var name = $"{topic} Announcements";
                var avatar = new MemoryStream(Images.DotaAvatar);
                if (announcement != null)
                {
                    webhook = await Context.Guild.GetWebhookAsync(announcement.WebhookId);
                    await webhook.ModifyAsync(w =>
                    {
                        w.Name = name;
                        w.Channel = channel;
                        w.Image = new Image(avatar);
                    });
                }
                else
                {
                    webhook = await channel.CreateWebhookAsync(name, avatar);
                    guild.Announcements.Add(new(topic, webhook.Id));
                    await _db.UpsertRecord(guild);
                }

                await FollowupAsync($"**{channel.GetChannelTypeIcon()}\u00A0{channel.Name}** will now receive {topic} updates!", ephemeral: true);
            }

            [ComponentInteraction("announcements:DEL:*")]
            public async Task AnnouncementsDelete(Topic topic)
            {
                await DeferAsync();

                var guild = await _db.GetGuild(Context.Guild);
                RestWebhook webhook;
                var announcement = guild.Announcements.FirstOrDefault(x => x.Topic == topic);
                if (announcement != null)
                {
                    webhook = await Context.Guild.GetWebhookAsync(announcement.WebhookId);
                    await webhook.DeleteAsync();
                    guild.Announcements.Remove(announcement);
                    await _db.UpsertRecord(guild);
                    // Add some error logic here and above in case the webhook is manually removed / updated, or database fails to update
                }

                await FollowupAsync($"Removed **{topic}** announcements from this server.", ephemeral: true);
            }
        }
    }
}
