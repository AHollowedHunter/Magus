using Magus.Common.Enums;
using Magus.Data.Enums;
using Magus.Data.Models;
using Magus.Data.Models.Discord;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using Magus.Data.Models.Magus;
using Microsoft.Extensions.Options;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;
using System.Security.Authentication;

namespace Magus.Data.Services;

public sealed class MongoDBService : IAsyncDataService
{
    private readonly DataSettings _config;
    private readonly MongoClient _client;
    private readonly IMongoDatabase _db;
    private bool _disposed;

    public MongoDBService(IOptions<DataSettings> config)
    {
        _config = config.Value;

        BsonChunkPool.Default = new BsonChunkPool(1024, 64 * 1024);

        MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(_config.ConnectionString));
        settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
        _client = new MongoClient(settings);
        _db = _client.GetDatabase(_config.DatabaseName);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
    }

    private IMongoCollection<T> GetCollection<T>()
        => _db.GetCollection<T>(typeof(T).Name);


    public void CreateCollection<T>() where T : ISnowflakeRecord
    {
        GetCollection<T>();
    }

    public async Task DeleteCollection<T>() where T : ISnowflakeRecord
    {
        await _db.DropCollectionAsync(typeof(T).Name);
    }

    public async Task<bool> DeleteRecord<T>(ulong id) where T : ISnowflakeRecord
    {
        var collection = GetCollection<T>();
        var result = await collection.DeleteOneAsync(x => x.Id == id);
        return result.IsAcknowledged && result.DeletedCount == 1;
    }

    public async Task<string> EnsureIndex<T>(Expression<Func<T, object>> field, bool unique = false, bool caseSensitive = true) where T : ISnowflakeRecord
    {
        var collection = GetCollection<T>();
        var indexKeysDefinition = Builders<T>.IndexKeys.Ascending(field);
        return await collection.Indexes.CreateOneAsync(new CreateIndexModel<T>(indexKeysDefinition,
                                                                               new()
                                                                               {
                                                                                   Unique = unique,
                                                                                   Collation = new Collation("simple",
                                                                                                             strength: caseSensitive ? CollationStrength.Tertiary : CollationStrength.Secondary)
                                                                               }));
    }

    public async Task<T> GetEntityInfo<T>(int entityId, string locale = "en-GB") where T : EntityInfoEmbed
    {
        var collection = GetCollection<T>();
        var result = await collection.FindAsync(x => x.Locale == locale && x.EntityId == entityId);
        return await result.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetEntityInfo<T>(string entityName, string locale = "en-GB", int limit = int.MaxValue) where T : EntityInfoEmbed
    {
        var collection = GetCollection<T>();
        return await collection.AsQueryable().Where(QueryLocaleEntityName<T>(entityName, locale)).OrderBy(x => x.InternalName).Take(limit).ToListAsync();
        //var query = QueryLocaleEntityNamex<T>(entityName, locale);
        //var result = await collection.FindAsync(query);
        //return await result.ToListAsync();
    }

    public async Task<GeneralPatchNoteEmbed> GetGeneralPatchNote(string patchNumber, string locale = "en-GB")
    {
        var collection = GetCollection<GeneralPatchNoteEmbed>();
        var result = await collection.FindAsync(x => x.Locale == locale && x.PatchNumber == patchNumber);
        return await result.FirstOrDefaultAsync();
    }

    public async Task<Patch> GetLatestPatch()
    {
        var collection = GetCollection<Patch>();
        return await collection.AsQueryable().OrderByDescending(x => x.Timestamp).FirstAsync();
    }

    public async Task<Patch> GetPatch(string patch)
    {
        var collection = GetCollection<Patch>();
        var result = await collection.FindAsync(x => x.PatchNumber == patch);
        return await result.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Patch>> GetPatches(string patch, int limit = int.MaxValue, bool orderByDesc = false)
    {
        var collection = GetCollection<Patch>();
        var query = collection.AsQueryable().Where(x => x.PatchNumber.StartsWith(patch));
        if (!orderByDesc)
            query = query.OrderBy(x => x.Timestamp);
        else
            query = query.OrderByDescending(x => x.Timestamp);
        return await query.Take(limit).ToListAsync();
    }

    public async Task<T> GetPatchNote<T>(string patchNumber, int entityId, string locale = "en-GB") where T : EntityPatchNoteEmbed
    {
        var collection = GetCollection<T>();
        var result = await collection.FindAsync(x => x.Locale == locale && x.PatchNumber == patchNumber && x.EntityId == entityId);
        return await result.FirstOrDefaultAsync();
    }

    public async Task<T> GetPatchNote<T>(string patchNumber, string entityName, string locale = "en-GB") where T : EntityPatchNoteEmbed
    {
        var collection = GetCollection<T>();
        var result = await collection.FindAsync(QueryPatchNoteEntityName<T>(entityName, patchNumber, locale));
        return await result.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetPatchNotes<T>(int entityId, string locale = "en-GB", int limit = int.MaxValue, bool orderByDesc = false) where T : EntityPatchNoteEmbed
    {
        var collection = GetCollection<T>();
        var query = collection.AsQueryable().Where(x => x.Locale == locale && x.EntityId == entityId);
        if (!orderByDesc)
            query = query.OrderBy(x => x.Timestamp);
        else
            query = query.OrderByDescending(x => x.Timestamp);
        return await query.Take(limit).ToListAsync();
    }

    public async Task<IEnumerable<T>> GetPatchNotes<T>(string entityName, string locale = "en-GB", int limit = int.MaxValue, bool orderByDesc = false) where T : EntityPatchNoteEmbed
    {
        var collection = GetCollection<T>();
        var query = collection.AsQueryable().Where(QueryLocaleEntityName<T>(entityName, locale)).OrderBy(x => x.InternalName);
        if (!orderByDesc)
            query = query.OrderBy(x => x.Timestamp);
        else
            query = query.OrderByDescending(x => x.Timestamp);
        return await query.Take(limit).ToListAsync();
    }

    public async Task<T> GetRecord<T>(ulong id) where T : ISnowflakeRecord
    {
        var collection = GetCollection<T>();
        var result = await collection.FindAsync(x => x.Id == id);
        return await result.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetRecords<T>(int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeRecord
    {
        var collection = GetCollection<T>();
        var result = await collection.FindAsync(x => x.Id != 0);
        return await result.ToListAsync();
    }

    public async Task<IEnumerable<T>> GetRecords<T>(string locale = "en-GB", int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeRecord, ILocaleRecord
    {
        var collection = GetCollection<T>();
        var query = collection.AsQueryable().Where(x => x.Locale == locale);
        if (!orderByDesc)
            query = query.OrderBy(x => x.Id);
        else
            query = query.OrderByDescending(x => x.Id);
        return await query.Take(limit).ToListAsync();
    }

    public async Task InsertRecord<T>(T record) where T : ISnowflakeRecord
    {
        var collection = GetCollection<T>();
        await collection.InsertOneAsync(record);
    }

    public async Task InsertRecords<T>(IEnumerable<T> records) where T : ISnowflakeRecord
    {
        var collection = GetCollection<T>();
        await collection.InsertManyAsync(records);
    }

    public async Task UpdateRecord<T>(T record) where T : ISnowflakeRecord
    {
        throw new NotImplementedException();
    }

    public async Task UpdateRecords<T>(IEnumerable<T> records) where T : ISnowflakeRecord
    {
        throw new NotImplementedException();
    }

    public async Task UpsertRecord<T>(T record) where T : ISnowflakeRecord
    {
        var collection = GetCollection<T>();
        await collection.ReplaceOneAsync(x => x.Id == record.Id, record, new ReplaceOptions() { IsUpsert = true });
    }

    public async Task UpsertRecords<T>(IEnumerable<T> records) where T : ISnowflakeRecord
    {
        var collection = GetCollection<T>();
        await collection.BulkWriteAsync(GetBulkReplaceRequest(records, true));
    }

    public async Task ReplaceRecord<T>(T record) where T : ISnowflakeRecord
    {
        var collection = GetCollection<T>();
        await collection.ReplaceOneAsync(x => x.Id == record.Id, record);
    }

    public async Task ReplaceRecords<T>(IEnumerable<T> records) where T : ISnowflakeRecord
    {
        var collection = GetCollection<T>();
        await collection.BulkWriteAsync(GetBulkReplaceRequest(records));
    }

    private static Expression<Func<T, bool>> QueryLocaleEntityName<T>(string entityName, string locale = "en-GB") where T : ILocalisedEntity, ILocaleRecord
        => entity => entity.Locale == locale && (entity.InternalName.Equals(entityName.ToLower())
                                                 || entity.Name.Contains(entityName, StringComparison.CurrentCultureIgnoreCase)
                                                 || entity.RealName!.StartsWith(entityName, StringComparison.CurrentCultureIgnoreCase)
                                                 || entity.InternalName.Contains(entityName, StringComparison.CurrentCultureIgnoreCase));

    private static Expression<Func<T, bool>> QueryPatchNoteEntityName<T>(string entityName, string patchNumber, string locale = "en-GB") where T : EntityPatchNoteEmbed
        => entity => entity.PatchNumber == patchNumber && entity.Locale == locale && (entity.InternalName.Equals(entityName.ToLower())
                                                                                      || entity.Name.Contains(entityName, StringComparison.CurrentCultureIgnoreCase)
                                                                                      || entity.RealName!.StartsWith(entityName, StringComparison.CurrentCultureIgnoreCase)
                                                                                      || entity.InternalName.Contains(entityName, StringComparison.CurrentCultureIgnoreCase));

    private static IEnumerable<WriteModel<T>> GetBulkReplaceRequest<T>(IEnumerable<T> records, bool isUpsert = false) where T : ISnowflakeRecord
    {
        var request = new List<WriteModel<T>>();
        foreach (var record in records)
            request.Add(new ReplaceOneModel<T>(Builders<T>.Filter.Where(x => x.Id == record.Id), record) { IsUpsert = isUpsert });
        return request;
    }

    public async Task<IEnumerable<Guild>> GetSubscribedGuilds(Topic topic)
    {
        var collection = GetCollection<Guild>();
        return await collection.AsQueryable().Where(g => g.Announcements.Any(a => a.Topic == topic)).ToListAsync();
    }

    public async Task<Announcement?> GetLatestPublishedAnnouncement(Topic topic)
    {
        var collection = GetCollection<Announcement>();
        return await collection.AsQueryable().OrderByDescending(a => a.Date).FirstOrDefaultAsync(a => a.IsPublished);
    }

    public async Task<int> GetTotalAnnouncementSubscriptions(Topic? topic = null)
    {
        var collection = GetCollection<Guild>();
        var query = collection.AsQueryable();
        if (topic != null)
            query = query.Where(g => g.Announcements.Any(g => g.Topic == topic));
        else
            query = query.Where(g => g.Announcements.Any());
        return await query.CountAsync();
    }

    public async Task<AbilityInfoEmbed> GetHeroScepter(int heroId, string locale = "en-GB")
    {
        var collection = GetCollection<AbilityInfoEmbed>();
        return await collection.AsQueryable().Where(ability => ability.HeroId == heroId && ability.Scepter && ability.Locale == locale).FirstOrDefaultAsync();
    }

    public async Task<AbilityInfoEmbed> GetHeroShard(int heroId, string locale = "en-GB")
    {
        var collection = GetCollection<AbilityInfoEmbed>();
        return await collection.AsQueryable().Where(ability => ability.HeroId == heroId && ability.Shard && ability.Locale == locale).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<EntityLocalisation>> GetEntityLocalisations(EntityType type)
    {
        var collection = GetCollection<EntityLocalisation>();
        return await collection.AsQueryable().Where(x => x.Type == type).ToListAsync();
    }
}
