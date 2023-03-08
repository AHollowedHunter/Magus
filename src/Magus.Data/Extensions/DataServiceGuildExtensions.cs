using Discord;
using Discord.WebSocket;
using Magus.Data.Enums;
using Magus.Data.Models.Discord;

namespace Magus.Data.Extensions
{
    public static class DataServiceGuildExtensions
    {
        /// <summary>
        /// This will create or update a Guild record, updating current information
        /// </summary>
        /// <param name="guild">Guild information to use</param>
        /// <param name="action">Whether to add a joined or left snapshot</param>
        /// <returns></returns>
        public async static Task UpsertGuildRecord(this IAsyncDataService db, SocketGuild guild, DiscordAction action = DiscordAction.None)
        {
            var guildRecord = await db.GetRecord<Guild>(guild.Id) ?? new Guild(guild.Id);

            guildRecord.CurrentName       = guild.Name;
            guildRecord.OwnerId           = guild.OwnerId;
            guildRecord.LatestMemberCount = guild.MemberCount;
            guildRecord.LastUpdated       = DateTime.UtcNow;

            guildRecord.IsCommunity    = guild.Features.HasFeature(GuildFeature.Community);
            guildRecord.IsDiscoverable = guild.Features.HasFeature(GuildFeature.Discoverable);
            guildRecord.IsFeatureable  = guild.Features.HasFeature(GuildFeature.Featureable);
            guildRecord.IsPartnered    = guild.Features.IsPartnered;
            guildRecord.IsVerified     = guild.Features.IsVerified;


            if (action == DiscordAction.Joined)
            {
                guildRecord.IsCurrentMember = true;
                guildRecord.JoinedInfo.Add(MakeSnapshot(guild, guild.CurrentUser.JoinedAt ?? DateTime.UtcNow));
            }
            if (action == DiscordAction.Left)
            {
                guildRecord.IsCurrentMember = false;
                guildRecord.LeftInfo.Add(MakeSnapshot(guild, DateTime.UtcNow));
            }

            await db.UpsertRecord(guildRecord);
        }

        public async static Task<Guild> GetGuild(this IAsyncDataService db, SocketGuild guild)
        {
            var guildRecord = await db.GetRecord<Guild>(guild.Id);
            if (guildRecord == null)
            {
                await db.UpsertGuildRecord(guild, DiscordAction.Joined);
                guildRecord = await db.GetRecord<Guild>(guild.Id);
            }
            return guildRecord;
        }


        private static Guild.Snapshot MakeSnapshot(SocketGuild guild, DateTimeOffset date)
            => new(date, guild.MemberCount, guild.Name, guild.OwnerId);
    }
}
