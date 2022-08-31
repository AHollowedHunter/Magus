using LiteDB;
using Magus.Data.Models;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Linq.Expressions;

namespace Magus.Data
{
    public class LiteDBService : IDatabaseService
    {
        private LiteDatabase _liteDB;
        private bool _disposed;

        public LiteDBService(IConfiguration config)
        {
            _liteDB = new LiteDatabase(config["ConnectionString"]);

            BsonMapper.Global.EnumAsInteger = true;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _liteDB.Dispose();
        }

        public void CreateCollection<T>() where T : ISnowflakeRecord
        {
            throw new NotImplementedException("LiteDB creates a collection when inserting a record");
        }

        public bool DeleteCollection<T>() where T : ISnowflakeRecord
        {
            return _liteDB.DropCollection(typeof(T).Name);
        }

        public ulong InsertRecord<T>(T record) where T : ISnowflakeRecord
        {
            var collection = _liteDB.GetCollection<T>();
            return (ulong)(long)collection.Insert(record);
        }

        public int InsertRecords<T>(IEnumerable<T> records) where T : ISnowflakeRecord
        {
            var collection = _liteDB.GetCollection<T>();
            return collection.InsertBulk(records);
        }

        public bool UpdateRecord<T>(T record) where T : ISnowflakeRecord
        {
            var collecton = _liteDB.GetCollection<T>();
            return collecton.Update(record);
        }

        public int UpdateRecords<T>(IEnumerable<T> records) where T : ISnowflakeRecord
        {
            var collecton = _liteDB.GetCollection<T>();
            return collecton.Update(records);
        }

        public bool UpsertRecord<T>(T record) where T : ISnowflakeRecord
        {
            var collecton = _liteDB.GetCollection<T>();
            return collecton.Upsert(record);
        }

        public int UpsertRecords<T>(IEnumerable<T> records) where T : ISnowflakeRecord
        {
            var collecton = _liteDB.GetCollection<T>();
            return collecton.Upsert(records);
        }

        public bool DeleteRecord<T>(ulong id) where T : ISnowflakeRecord
        {
            var liteId = (long)id;
            var collecton = _liteDB.GetCollection<T>();
            return collecton.Delete(liteId);
        }

        public T GetRecord<T>(ulong id) where T : ISnowflakeRecord
        {
            var liteId = (long)id;
            var collecton = _liteDB.GetCollection<T>();
            return collecton.FindById(liteId);
        }

        public IEnumerable<T> GetRecords<T>(int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeRecord
        {
            var order = orderByDesc ? Query.Descending : Query.Ascending;
            var collecton = _liteDB.GetCollection<T>();
            return collecton.Find(Query.All(order), limit: limit);
        }

        public IEnumerable<T> GetRecords<T>(ulong id, int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeRecord
        {
            var liteId = (long)id;
            var collection = _liteDB.GetCollection<T>();
            IEnumerable<T> records;
            if (!orderByDesc)
            {
                records = collection.Find(Query.GTE("Id", liteId)).OrderBy(x => x.Id);
            }
            else
            {
                records = collection.Find(Query.LTE("Id", liteId)).OrderByDescending(x => x.Id);
            }
            return records.Take(limit);
        }

        public IEnumerable<T> GetRecords<T>(string locale = IDatabaseService.DEFAULT_LOCALE, int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeRecord, ILocaleRecord
        {
            var order = orderByDesc ? Query.Descending : Query.Ascending;
            var collecton = _liteDB.GetCollection<T>();
            var results = collecton.Find(record => record.Locale == locale, limit: limit);
            return orderByDesc !? results : results.OrderByDescending(x => x.Id);
        }

        public IEnumerable<T> GetRecords<T>(ulong id, string locale = IDatabaseService.DEFAULT_LOCALE, int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeRecord, ILocaleRecord
        {
            var liteId = (long)id;
            var collection = _liteDB.GetCollection<T>();
            IEnumerable<T> records;
            if (!orderByDesc)
            {
                records = collection.Find(Query.And(Query.EQ("locale", locale), Query.GTE("Id", liteId))).OrderBy(x => x.Id);
            }
            else
            {
                records = collection.Find(Query.And(Query.EQ("locale", locale), Query.LTE("Id", liteId))).OrderByDescending(x => x.Id);
            }
            return records.Take(limit);
        }

        public Patch GetLatestPatch()
        {
            var collection = _liteDB.GetCollection<Patch>();
            var results = collection.FindAll().OrderByDescending(x => x.Timestamp).First();
            return results;
        }

        public Patch GetPatch(string patchNumber)
        {
            var collection = _liteDB.GetCollection<Patch>();
            var results = collection.FindOne(Query.EQ("PatchNumber", patchNumber));
            return results;
        }

        public IEnumerable<Patch> GetPatches(string patchNumber, int limit = int.MaxValue, bool orderByDesc = false)
        {
            var collection = _liteDB.GetCollection<Patch>();
            var results = collection.Find(Query.StartsWith("PatchNumber", patchNumber));
            if (!orderByDesc)
            {
                results = results.OrderBy(x => x.Timestamp);
            }
            else
            {
                results = results.OrderByDescending(x => x.Timestamp);
            }
            return results.Take(limit);
        }

        public GeneralPatchNoteEmbed GetGeneralPatchNote(string patchNumber, string locale = IDatabaseService.DEFAULT_LOCALE)
        {
            var collection = _liteDB.GetCollection<GeneralPatchNoteEmbed>();
            var results = collection.FindOne(Query.And(Query.EQ("PatchNumber", patchNumber), Query.EQ("Locale", locale)));
            return results;
        }

        public IEnumerable<T> GetPatchNotes<T>(int entityId, string locale = IDatabaseService.DEFAULT_LOCALE, int limit = int.MaxValue, bool orderByDesc = false) where T : EntityPatchNoteEmbed
        {
            var collection = _liteDB.GetCollection<T>();
            var results = collection.Find(Query.And(Query.EQ("EntityId", entityId), Query.EQ("Locale", locale)));
            if (!orderByDesc)
            {
                results = results.OrderBy(x => x.PatchNumber);
            }
            else
            {
                results = results.OrderByDescending(x => x.PatchNumber);
            }
            return results.Take(limit);
        }

        public IEnumerable<T> GetPatchNotes<T>(string entityName, string locale = IDatabaseService.DEFAULT_LOCALE, int limit = int.MaxValue, bool orderByDesc = false) where T : EntityPatchNoteEmbed
        {
            var collection = _liteDB.GetCollection<T>();
            return collection.Find(QueryLocaleEntityName<T>(entityName, locale), limit: limit);
        }

        public T GetPatchNote<T>(string patchNumber, int entityId) where T : EntityPatchNoteEmbed
        {
            var collection = _liteDB.GetCollection<T>();
            var result = collection.FindOne(Query.And(Query.EQ("PatchNumber", patchNumber), Query.EQ("EntityId", entityId)));
            return result;
        }

        public IEnumerable<T> GetPatchNote<T>(string patchNumber, string entityName, int limit = int.MaxValue) where T : EntityPatchNoteEmbed
        {
            var collection = _liteDB.GetCollection<T>();
            return collection.Find(QueryPatchNoteEntityName<T>(entityName, patchNumber), limit: limit);
        }

        public T GetEntityInfo<T>(int entityId, string locale = IDatabaseService.DEFAULT_LOCALE) where T : EntityInfoEmbed
        {
            var collection = _liteDB.GetCollection<T>();
            var result = collection.FindOne(Query.And(Query.EQ("EntityId", entityId), Query.EQ("Locale", locale)));
            return result;
        }

        public IEnumerable<T> GetEntityInfo<T>(string entityName, string locale = IDatabaseService.DEFAULT_LOCALE, int limit = int.MaxValue) where T : EntityInfoEmbed
        {
            var collection = _liteDB.GetCollection<T>();
            return collection.Find(QueryLocaleEntityName<T>(entityName, locale), limit: limit);
        }

        public bool EnsureIndex<T>(string fieldName, bool unique = false) where T : ISnowflakeRecord
        {
            var collection = _liteDB.GetCollection<T>();
            if (collection == null)
                return false;
            return collection.EnsureIndex(fieldName, unique);
        }

        private static Expression<Func<T, bool>> QueryLocaleEntityName<T>(string entityName, string locale = IDatabaseService.DEFAULT_LOCALE) where T : INamedEntity, ILocaleRecord
            => entity => entity.Locale == locale && (entity.Name!.Contains(entityName)
                                                                       || entity.RealName!.StartsWith(entityName)
                                                                       || entity.Aliases!.Any(alias => alias.StartsWith(entityName, StringComparison.InvariantCultureIgnoreCase))
                                                                       || entity.InternalName!.Contains(entityName));

        private static Expression<Func<T, bool>> QueryPatchNoteEntityName<T>(string entityName, string patchNumber) where T : EntityPatchNoteEmbed
            => entity => entity.PatchNumber == patchNumber && (entity.Name!.Contains(entityName)
                                                                       || entity.RealName!.StartsWith(entityName)
                                                                       || entity.Aliases!.Any(alias => alias.StartsWith(entityName, StringComparison.InvariantCultureIgnoreCase))
                                                                       || entity.InternalName!.Contains(entityName));
    }
}
