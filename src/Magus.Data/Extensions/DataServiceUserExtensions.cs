using Discord.WebSocket;
using Magus.Data.Enums;
using Magus.Data.Models.Discord;

namespace Magus.Data.Extensions
{
    public static class DataServiceUserExtensions
    {
        /// <summary>
        /// This will create or update a User record, updating current information
        /// </summary>
        /// <param name="user">User information to use</param>
        /// <returns></returns>
        public async static Task UpsertUserRecord(this IAsyncDataService db, SocketUser user, DiscordGuildAction action = DiscordGuildAction.None)
        {
            var userRecord = await db.GetRecord<User>(user.Id) ?? new User(user.Id);

            userRecord.LastUpdated = DateTime.UtcNow;

            await db.UpsertRecord(userRecord);
        }

        public async static Task<User> GetUser(this IAsyncDataService db, SocketUser user)
        {
            var userRecord = await db.GetRecord<User>(user.Id);
            if (userRecord == null)
            {
                await db.UpsertUserRecord(user, DiscordGuildAction.Joined);
                userRecord = await db.GetRecord<User>(user.Id);
            }
            return userRecord;
        }

        /// <summary>
        /// Returns true is user doesn't exist or was successfully delete, false if it failed.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async static Task<bool> DeleteUser(this IAsyncDataService db, SocketUser user)
        {
            var userRecord = await db.GetRecord<User>(user.Id);
            if (userRecord != null)
            {
                return await db.DeleteRecord<User>(userRecord.Id);
            }
            return true;
        }
    }
}
