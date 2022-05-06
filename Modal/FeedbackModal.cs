using Discord;
using Discord.Interactions;
using Magus.Data.Models.Magus;

namespace Magus.Bot.Modal
{
    public abstract class FeedbackModalBase :IModal
    {
        public abstract FeedbackType Type { get; }

        public string Title => $"Give feedback - {Type}";

        [ModalTextInput("Message", TextInputStyle.Paragraph, "Type your feedback here")]
        public string Message { get; set; }

        public Feedback ToFeedback()
            => new() { Type = Type, Message = Message };
    }

    public class FeatureFeedbackModal : FeedbackModalBase
    {
        public override FeedbackType Type { get => FeedbackType.Feature; }
    }

    public class BugFeedbackModal : FeedbackModalBase
    {
        public override FeedbackType Type { get => FeedbackType.Bug; }
    }

    public class OtherFeedbackModal : FeedbackModalBase
    {
        public override FeedbackType Type { get => FeedbackType.Other; }
    }
}
