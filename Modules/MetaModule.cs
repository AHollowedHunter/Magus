using Discord;
using Discord.Interactions;
using Magus.Bot.Modal;
using Magus.Data;
using Magus.Data.Models.Magus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Magus.Bot.Modules
{
    [Group("magus", "all things MagusBot")]
    public class MetaModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IDatabaseService _db;
        private readonly IConfiguration _config;
        private readonly ILogger<MetaModule> _logger;
        private readonly Services.IWebhook _webhook;

        string inviteLink => _config["BotInvite"];

        public MetaModule(IDatabaseService db, IConfiguration config, ILogger<MetaModule> logger, Services.IWebhook webhook)
        {
            _db = db;
            _config = config;
            _logger = logger;
            _webhook = webhook;
        }

        string version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        DateTimeOffset versionDate = DateTimeOffset.Now;

        [SlashCommand("about", "About this bot ℹ")]
        public async Task About()
        {
            var author = await Context.Client.GetUserAsync(240463688627126278);
            var latestPatchNote = _db.GetLatestPatch();
            var latestPatch = $"[{ latestPatchNote.PatchNumber }](https://www.dota2.com/patches/{latestPatchNote.PatchNumber}) - <t:{latestPatchNote.PatchTimestamp}:R>";
            var response = new EmbedBuilder()
            {
                Title = "MagusBot",
                Description = "A DotA 2 Discord bot",
                Author = new EmbedAuthorBuilder() { Name = "AHollowedHunter", Url = $"https://github.com/AHollowedHunter", IconUrl = "https://avatars.githubusercontent.com/u/45659989?v=4&s=48" },
                Color = Color.Purple,
                Timestamp = versionDate,
                Footer = new() { Text = "Hot Damn!", IconUrl = Context.Client.CurrentUser.GetAvatarUrl() },
            };
            response.AddField(new EmbedFieldBuilder() { Name = "Version", Value = version, IsInline = true });
            response.AddField(new EmbedFieldBuilder() { Name = "Latest Patch", Value = latestPatch, IsInline = true });
            response.AddField(new EmbedFieldBuilder() { Name = "Total Guilds", Value = Context.Client.Guilds.Count(), IsInline = false });
            //response.AddField(new EmbedFieldBuilder() { Name = "Latency", Value = Context.Client.Latency + "ms", IsInline = true });
            //response.AddField(new EmbedFieldBuilder() { Name = "ShardId", Value = Context.Client.ShardId, IsInline = true });

            var links = $"[Invite Link]({inviteLink})\n[MagusBot.xyz](https://magusbot.xyz)\n";
            response.AddField(new EmbedFieldBuilder() { Name = "Links", Value = links, IsInline = false });

            await RespondAsync(embed: response.Build(), ephemeral: true);
        }

        [SlashCommand("invite", "Where shall I go next? Ultimyr University? Yama Raskav? Hmm.")]
        public async Task Invite()
        {
            await RespondAsync(text: "Share me with your friends (or server admin) with my invite link!\n" + inviteLink);
        }

        [SlashCommand("help", "Get help using MagusBot")]
        public async Task Help()
        {
            List<IApplicationCommand> commands = new List<IApplicationCommand>();
            commands.AddRange(await Context.Guild.GetApplicationCommandsAsync());
            commands.AddRange(await Context.Client.Rest.GetGlobalApplicationCommands());

            var embeds = new List<Embed>();

            foreach (IApplicationCommand command in commands)
            {
                if (command.Type != ApplicationCommandType.Slash) continue;

                var embed = new EmbedBuilder()
                {
                    Title = command.Name,
                    Description = command.Description,
                    Color = Color.DarkGreen,
                    Timestamp = versionDate,
                    Footer = new() { Text = $"MagusBot version {version}" }
                };

                if (!command.Options.Any(x => x.Type == ApplicationCommandOptionType.SubCommand))
                {
                    embed.Description += $"\nTry it: </{command.Name}:{command.Id}>\n";
                }
                else
                {
                    foreach (var option in command.Options)
                    {
                        var field = new EmbedFieldBuilder()
                        {
                            Name = $"/{command.Name} {option.Name}",
                        };
                        var value = $"{option.Description}\n";
                        if (option.Type == ApplicationCommandOptionType.SubCommand)
                        {
                            value += $"Try it: </{command.Name} {option.Name}:{command.Id}>\n";
                        }
                        else
                        {
                            foreach (var optionLevelTwo in option.Options)
                            {
                                if (optionLevelTwo.Type == ApplicationCommandOptionType.SubCommand)
                                {
                                    var fullCommand = $"{command.Name} {option.Name} {optionLevelTwo.Name}";
                                    value += $"**/{fullCommand}**\n*{optionLevelTwo.Description}*\nTry it: </{fullCommand}:{command.Id}>\n";
                                }
                            }
                        }
                        field.WithValue(value);
                        embed.AddField(field);
                    }
                }
                embeds.Add(embed.Build());
            }
            await RespondAsync(embeds: embeds.ToArray(), ephemeral: true);
        }

        [SlashCommand("feedback", "Got a bug or suggestion? Give it here!")]
        public async Task Feedback()
        {
            var selectMenuBuilder = new SelectMenuBuilder()
                .WithPlaceholder("Select the type of feedback")
                .WithCustomId("feedback-type")
                .AddOption("Feature", FeedbackType.Feature.ToString())
                .AddOption("Bug", FeedbackType.Bug.ToString())
                .AddOption("Other", FeedbackType.Other.ToString())
                .WithMinValues(1)
                .WithMaxValues(1);

            var builder = new ComponentBuilder()
                .WithSelectMenu(selectMenuBuilder);

            await RespondAsync(components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("feedback-type", ignoreGroupNames: true)]
        public async Task HandleFeedbackSelect(string input)
        {
            Enum.TryParse(input, out FeedbackType type);
            switch (type)
            {
                case FeedbackType.Feature:
                    await Context.Interaction.RespondWithModalAsync<FeatureFeedbackModal>("feature-feedback");
                    break;
                case FeedbackType.Bug:
                    await Context.Interaction.RespondWithModalAsync<BugFeedbackModal>("bug-feedback");
                    break;
                case FeedbackType.Other:
                    await Context.Interaction.RespondWithModalAsync<OtherFeedbackModal>("feedback");
                    break;
            }
        }

        [ModalInteraction("feature-feedback", ignoreGroupNames: true)]
        public async Task FeatureFeedbackResponse(FeatureFeedbackModal feedback)
        {
            var feedbackId = await SaveFeedback(feedback);
            await FeedbackResponse(feedbackId);

        }

        [ModalInteraction("bug-feedback", ignoreGroupNames: true)]
        public async Task BugFeedbackResponse(BugFeedbackModal feedback)
        {
            var feedbackId = await SaveFeedback(feedback);
            await FeedbackResponse(feedbackId);
        }

        [ModalInteraction("feedback", ignoreGroupNames: true)]
        public async Task FeedbackResponse(OtherFeedbackModal feedback)
        {
            var feedbackId = await SaveFeedback(feedback);
            await FeedbackResponse(feedbackId);
        }

        private async Task<ulong> SaveFeedback<T>(T feedbackModal) where T : FeedbackModalBase
        {
            var feedback = feedbackModal.ToFeedback();
            if (Context.Interaction.IsDMInteraction)
            {
                feedback.IsDMSubmitted = true;
            }
            else
            {
                feedback.GuildSubmitted = Context.Guild.Id;
            }
            feedback.Author = Context.Interaction.User.Id;
            feedback.Id = MakeFeedbackId(feedback.Author);
            
            var id = _db.InsertRecord(feedback);
            if (id != 0xFFFFFFFFFFFFFFFF)
            {
                var success = await _webhook.SendMessage(CreateFeedbackMessage(feedback), _config["FeedbackWebhook"]);
                if (!success) _logger.LogWarning("Failed to send webhook for feedback #{}", id);
            }
            return id;
        }

        private static ulong MakeFeedbackId(ulong authorId)
           => (((ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds() - 1651363200000) << 22) + (authorId & 0x2FFFFF);

        private async Task FeedbackResponse(ulong feedbackId)
        {
            if (feedbackId != 0xFFFFFFFFFFFFFFFF)
            {
                await RespondAsync(text: "Thank you for your feedback.\nFeedback ID: " + feedbackId, ephemeral: true);
            }
            else
            {
                await RespondAsync(text: "Failed to save feedback, please try again in a minute", ephemeral: true);
            }
        }

        private string CreateFeedbackMessage(Feedback feedback)
        {
            return @$"{{""embeds"": [{{""title"": ""{feedback.Type} #{feedback.Id}"",""description"": ""{feedback.Message}""}}]}}";
        }
    }
}
