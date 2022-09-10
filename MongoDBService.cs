using Magus.Common;
using Magus.Data.Models;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Security.Authentication;
using System.Linq.Expressions;
using MongoDB.Bson.IO;

namespace Magus.Data
{
    public class MongoDBService : IAsyncDataService
    {
        private readonly Configuration _config;
        private MongoClient _client;
        private IMongoDatabase _db;
        private bool _disposed;

        public MongoDBService(Configuration config)
        {
            _config = config;

            BsonChunkPool.Default = new BsonChunkPool(1024, 64 * 1024);

            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(_config.DatabaseService.ConnectionString));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            _client = new MongoClient(settings);
            _db = _client.GetDatabase("test");
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

        public async Task<string> EnsureIndex<T>(Expression<Func<T, object>> field, bool unique = false) where T : ISnowflakeRecord
        {
            var collection = GetCollection<T>();
            var indexKeysDefinition = Builders<T>.IndexKeys.Ascending(field);
            return await collection.Indexes.CreateOneAsync(new CreateIndexModel<T>(indexKeysDefinition, new() { Unique = unique }));
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
            return await collection.AsQueryable().Where(QueryLocaleEntityName<T>(entityName, locale)).Take(limit).ToListAsync();
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
                query.OrderBy(x => x.Timestamp);
            else
                query.OrderByDescending(x => x.Timestamp);
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
                query.OrderBy(x => x.Timestamp);
            else
                query.OrderByDescending(x => x.Timestamp);
            return await query.Take(limit).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetPatchNotes<T>(string entityName, string locale = "en-GB", int limit = int.MaxValue, bool orderByDesc = false) where T : EntityPatchNoteEmbed
        {
            var collection = GetCollection<T>();
            var query = collection.AsQueryable().Where(QueryLocaleEntityName<T>(entityName, locale));
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
                query.OrderBy(x => x.Id);
            else
                query.OrderByDescending(x => x.Id);
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

        private static FilterDefinition<T> FilterLocaleEntityName<T>(string entityName, string locale = IDatabaseService.DEFAULT_LOCALE) where T : INamedEntity, ILocaleRecord
            => Builders<T>.Filter.And(Builders<T>.Filter.Where(x => x.Locale == locale),
                                      Builders<T>.Filter.Where(x => x.InternalName.StartsWith(entityName) || x.Name.StartsWith(entityName) || x.RealName!.StartsWith(entityName)));
        //private static FilterDefinition<T> QueryLocaleEntityNamex<T>(string entityName, string locale = IDatabaseService.DEFAULT_LOCALE) where T : INamedEntity, ILocaleRecord
        //    => Builders<T>.Filter.And(Builders<T>.Filter.Where(x => x.Locale == locale),
        //                              Builders<T>.Filter.Or(
        //                                  Builders<T>.Filter.Where(x => x.InternalName.StartsWith(entityName) || x.Name.StartsWith(entityName) || x.RealName!.StartsWith(entityName)),
        //                                  Builders<T>.Filter.ElemMatch(entity => entity.Aliases, alias => alias.StartsWith(entityName))));

        private static Expression<Func<T, bool>> QueryLocaleEntityName<T>(string entityName, string locale = IDatabaseService.DEFAULT_LOCALE) where T : INamedEntity, ILocaleRecord
            => entity => entity.Locale == locale && (entity.Name!.Contains(entityName)
                                                                       || entity.RealName!.StartsWith(entityName)
                                                                       //|| entity.Aliases!.Any(alias => alias.StartsWith(entityName, StringComparison.InvariantCultureIgnoreCase))
                                                                       || entity.InternalName!.Contains(entityName));

        private static Expression<Func<T, bool>> QueryPatchNoteEntityName<T>(string entityName, string patchNumber, string locale = IDatabaseService.DEFAULT_LOCALE) where T : EntityPatchNoteEmbed
            => entity => entity.PatchNumber == patchNumber && entity.Locale == locale && (entity.Name!.Contains(entityName)
                                                                                          || entity.RealName!.StartsWith(entityName)
                                                                                          //|| entity.Aliases!.Any(alias => alias.StartsWith(entityName, StringComparison.InvariantCultureIgnoreCase))
                                                                                          || entity.InternalName!.Contains(entityName));

        private IEnumerable<WriteModel<T>> GetBulkReplaceRequest<T>(IEnumerable<T> records, bool isUpsert = false) where T : ISnowflakeRecord
        {
            var request = new List<WriteModel<T>>();
            foreach (var record in records)
                request.Add(new ReplaceOneModel<T>(Builders<T>.Filter.Where(x => x.Id == record.Id), record) { IsUpsert = isUpsert });
            return request;
        }
    }
}
