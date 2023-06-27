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
        public bool IsCommunity { get; set; }
        public bool IsDiscoverable { get; set; }
        public bool IsFeatureable { get; set; }
        public bool IsPartnered { get; set; }
        public bool IsVerified { get; set; }
        public bool IsCurrentMember { get; set; } = true;
        public bool HideDpcSpoilers { get; set; } = true;
        public bool HasBeenToldOfSpoilers { get; set; } = false;
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
        public IList<Snapshot> JoinedInfo { get; init; } = new List<Snapshot>();
        public IList<Snapshot> LeftInfo { get; init; } = new List<Snapshot>();
        public IList<Announcement> Announcements { get; init; } = new List<Announcement>();

        // Sub-records
        public record Snapshot(DateTimeOffset Date, int MemberCount, string Name, ulong OwnerId);

        public record Announcement(Topic Topic, ulong WebhookId);

        public record GuildConfig(bool DpcSpoilers);
    }
}
