using Magus.Common.Enums;
using Magus.Data.Enums;
using Magus.Data.Models;
using Magus.Data.Models.Discord;
using Magus.Data.Models.Magus;
using System.Linq.Expressions;

namespace Magus.Data.Services;

public interface IAsyncDataService
{
    void CreateCollection<T>() where T : ISnowflakeId;
    Task DeleteCollection<T>() where T : ISnowflakeId;
    Task<bool> DeleteRecord<T>(ulong id) where T : ISnowflakeId;
    Task<string> EnsureIndex<T>(Expression<Func<T, object>> field, bool unique = false, bool caseSensitive = true) where T : ISnowflakeId;
    Task<IEnumerable<EntityLocalisation>> GetEntityLocalisations(EntityType type);
    Task<Announcement?> GetLatestPublishedAnnouncement(Topic topic);
    Task<T> GetRecord<T>(ulong id) where T : ISnowflakeId;
    Task<IEnumerable<T>> GetRecords<T>(int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeId;
    Task<IEnumerable<T>> GetRecords<T>(string locale = "en", int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeId, ILocaleRecord;
    Task<IEnumerable<Guild>> GetSubscribedGuilds(Topic topic);
    Task<int> GetTotalAnnouncementSubscriptions(Topic? topic = null);
    Task InsertRecord<T>(T record) where T : ISnowflakeId;
    Task InsertRecords<T>(IEnumerable<T> records) where T : ISnowflakeId;
    Task ReplaceRecord<T>(T record) where T : ISnowflakeId;
    Task ReplaceRecords<T>(IEnumerable<T> records) where T : ISnowflakeId;
    Task UpdateRecord<T>(T record) where T : ISnowflakeId;
    Task UpdateRecords<T>(IEnumerable<T> records) where T : ISnowflakeId;
    Task UpsertRecord<T>(T record) where T : ISnowflakeId;
    Task UpsertRecords<T>(IEnumerable<T> records) where T : ISnowflakeId;
}
