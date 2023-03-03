using Magus.Common.Enums;

namespace Magus.Data.Models.Discord
{
    public record Guild : ISnowflakeRecord
    {
        public Guild() { }
        public Guild(ulong id) => Id = id;

        public ulong Id { get; set; }
        public string CurrentName { get; set; }
        public ulong OwnerId { get; set; }
        public int LatestMemberCount { get; set; }
        public bool IsCurrentMember { get; set; } = true;

        public IList<Snapshot> JoinedInfo { get; init; } = new List<Snapshot>();
        public IList<Snapshot> LeftInfo { get; init; } = new List<Snapshot>();

        public record Snapshot(DateTimeOffset Date, int MemberCount, string Name, ulong OwnerId);

        public IList<Announcement> Announcements { get; init; } = new List<Announcement>();

        public record Announcement(Topic Topic, ulong WebhookId);
    }
}
