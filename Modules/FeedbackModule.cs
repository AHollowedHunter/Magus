using Discord.Interactions;
using Discord;
using Magus.Bot.Attributes;
using Magus.Bot.Modal;
using Magus.Data.Models.Magus;
using Magus.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Magus.Bot.Modules
{
    [ModuleRegistration(Location.TESTING, false)]
    public class FeedbackModule : ModuleBase
    {
        private readonly IDatabaseService _db;
        private readonly IConfiguration _config;
        private readonly ILogger<MetaModule> _logger;
        private readonly Services.IWebhook _webhook;

        public FeedbackModule(IDatabaseService db, IConfiguration config, ILogger<MetaModule> logger, Services.IWebhook webhook)
        {
            _db = db;
            _config = config;
            _logger = logger;
            _webhook = webhook;
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
                if (!success) _logger.LogWarning("Failed to send webhook for feedback #{id}", id);
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
