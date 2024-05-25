﻿using Magus.Common.Enums;
using Magus.Data.Enums;
using Magus.Data.Models;
using Magus.Data.Models.Discord;
using Magus.Data.Models.Dota;
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


    public void CreateCollection<T>() where T : ISnowflakeId
    {
        GetCollection<T>();
    }

    public async Task DeleteCollection<T>() where T : ISnowflakeId
    {
        await _db.DropCollectionAsync(typeof(T).Name);
    }

    public async Task<bool> DeleteRecord<T>(ulong id) where T : ISnowflakeId
    {
        var collection = GetCollection<T>();
        var result = await collection.DeleteOneAsync(x => x.Id == id);
        return result.IsAcknowledged && result.DeletedCount == 1;
    }

    public async Task<string> EnsureIndex<T>(Expression<Func<T, object>> field, bool unique = false, bool caseSensitive = true) where T : ISnowflakeId
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

    public async Task<T> GetRecord<T>(ulong id) where T : ISnowflakeId
    {
        var collection = GetCollection<T>();
        var result = await collection.FindAsync(x => x.Id == id);
        return await result.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetRecords<T>(int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeId
    {
        var collection = GetCollection<T>();
        var result = await collection.FindAsync(x => x.Id != 0);
        return await result.ToListAsync();
    }

    public async Task InsertRecord<T>(T record) where T : ISnowflakeId
    {
        var collection = GetCollection<T>();
        await collection.InsertOneAsync(record);
    }

    public async Task InsertRecords<T>(IEnumerable<T> records) where T : ISnowflakeId
    {
        var collection = GetCollection<T>();
        await collection.InsertManyAsync(records);
    }

    public async Task UpdateRecord<T>(T record) where T : ISnowflakeId
    {
        throw new NotImplementedException();
    }

    public async Task UpdateRecords<T>(IEnumerable<T> records) where T : ISnowflakeId
    {
        throw new NotImplementedException();
    }

    public async Task UpsertRecord<T>(T record) where T : ISnowflakeId
    {
        var collection = GetCollection<T>();
        await collection.ReplaceOneAsync(x => x.Id == record.Id, record, new ReplaceOptions() { IsUpsert = true });
    }

    public async Task UpsertRecords<T>(IEnumerable<T> records) where T : ISnowflakeId
    {
        var collection = GetCollection<T>();
        await collection.BulkWriteAsync(GetBulkReplaceRequest(records, true));
    }

    public async Task ReplaceRecord<T>(T record) where T : ISnowflakeId
    {
        var collection = GetCollection<T>();
        await collection.ReplaceOneAsync(x => x.Id == record.Id, record);
    }

    public async Task ReplaceRecords<T>(IEnumerable<T> records) where T : ISnowflakeId
    {
        var collection = GetCollection<T>();
        await collection.BulkWriteAsync(GetBulkReplaceRequest(records));
    }

    private static IEnumerable<WriteModel<T>> GetBulkReplaceRequest<T>(IEnumerable<T> records, bool isUpsert = false) where T : ISnowflakeId
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

    public async Task<IEnumerable<EntityLocalisation>> GetEntityLocalisations(EntityType type)
    {
        var collection = GetCollection<EntityLocalisation>();
        return await collection.AsQueryable().Where(x => x.Type == type).ToListAsync();
    }
}