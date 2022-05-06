namespace Magus.Data.Models.Magus
{
    public record Feedback : ISnowflakeRecord
    {
        public ulong Id { get; set; }
        public ulong Author { get; set; }
        public string Message { get; set; }
        public FeedbackType Type { get; set; }
        public string Resolution { get; set; }
        public bool IsClosed { get; set; }
        public bool IsDeleted { get; set; }
    }

    public enum FeedbackType
    {
        Other,
        Bug,
        Feature,
    }
}
