using LiteDB;
using Magus.Data.Models;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using Microsoft.Extensions.Configuration;
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

        public void CreateCollection<T>() where T : IGuidRecord, ISnowflakeRecord
        {
            throw new NotImplementedException("LiteDB creates a collection when inserting a record");
        }

        public bool DeleteCollection<T>() where T : IGuidRecord, ISnowflakeRecord
        {
            return _liteDB.DropCollection(typeof(T).Name);
        }

        public void InsertRecord<T>(T record) where T : IGuidRecord, ISnowflakeRecord
        {
            var collection = _liteDB.GetCollection<T>();
            collection.Insert(record);
        }

        public int InsertRecords<T>(IEnumerable<T> records) where T : IGuidRecord, ISnowflakeRecord
        {
            var collection = _liteDB.GetCollection<T>();
            return collection.InsertBulk(records);
        }

        public bool UpdateRecord<T>(T record) where T : IGuidRecord, ISnowflakeRecord
        {
            var collecton = _liteDB.GetCollection<T>();
            return collecton.Update(record);
        }

        public int UpdateRecords<T>(IEnumerable<T> records) where T : IGuidRecord, ISnowflakeRecord
        {
            var collecton = _liteDB.GetCollection<T>();
            return collecton.Update(records);
        }

        public bool DeleteRecord<T>(Guid id) where T : IGuidRecord
        {
            var collecton = _liteDB.GetCollection<T>();
            return collecton.Delete(id);
        }

        public bool DeleteRecord<T>(ulong id) where T : ISnowflakeRecord
        {
            var collecton = _liteDB.GetCollection<T>();
            return collecton.Delete(id);
        }

        public T GetRecord<T>(Guid id) where T : IGuidRecord
        {
            var collecton = _liteDB.GetCollection<T>();
            return collecton.FindById(id);
        }

        public T GetRecord<T>(ulong id) where T : ISnowflakeRecord
        {
            var collecton = _liteDB.GetCollection<T>();
            return collecton.FindById(id);
        }

        public IEnumerable<T> GetRecords<T>(int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeRecord
        {
            var order = orderByDesc ? Query.Descending : Query.Ascending;
            var collecton = _liteDB.GetCollection<T>();
            return collecton.Find(Query.All(order), limit: limit);
        }

        public IEnumerable<T> GetRecords<T>(ulong id, int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeRecord
        {
            var collection = _liteDB.GetCollection<T>();
            IEnumerable<T> records;
            if (!orderByDesc)
            {
                records = collection.Find(Query.GTE("Id", id.ToString())).OrderBy(x => x.Id);
            }
            else
            {
                records = collection.Find(Query.LTE("Id", id.ToString())).OrderByDescending(x => x.Id);
            }
            return records.Take(limit);
        }

        public Patch GetLatestPatch()
        {
            var collection = _liteDB.GetCollection<Patch>();
            var results = collection.FindAll().OrderByDescending(x => x.PatchTimestamp).First();
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
                results = results.OrderBy(x => x.PatchTimestamp);
            }
            else
            {
                results = results.OrderByDescending(x => x.PatchTimestamp);
            }
            return results.Take(limit);
        }

        public GeneralPatchNote GetGeneralPatchNote(string patchNumber)
        {
            var collection = _liteDB.GetCollection<GeneralPatchNote>();
            var results = collection.FindOne(Query.EQ("PatchNumber", patchNumber));
            return results;
        }

        public IEnumerable<T> GetPatchNotes<T>(int entityId, int limit = int.MaxValue) where T : EntityPatchNote
        {
            var collection = _liteDB.GetCollection<T>();
            var results = collection.Find(Query.EQ("EntityId", entityId)).Take(limit);
            return results;
        }

        public IEnumerable<T> GetPatchNotes<T>(string entityName, int limit = int.MaxValue) where T : EntityPatchNote
        {
            var collection = _liteDB.GetCollection<T>();
            IEnumerable<T> results;
            results = collection.Find(Query.Contains("LocalName", entityName), limit: limit);
            results.Concat(collection.Find(Query.Contains("RealName", entityName), limit: limit));
            results.Concat(collection.Find(Query.Contains("InternalName", entityName), limit: limit));
            results.Concat(collection.Find(Query.Contains("Aliases[*]", entityName), limit: limit));
            return results.Take(limit);
        }

        public IEnumerable<T> GetPatchNotes<T>(string patchNumber, int entityId, int limit = int.MaxValue) where T : EntityPatchNote
        {
            var collection = _liteDB.GetCollection<T>();
            var results = collection.Find(Query.Or(Query.EQ("PatchNumber", patchNumber), Query.EQ("EntityId", entityId)), limit: limit);
            return results;
        }

        public IEnumerable<T> GetPatchNotes<T>(string patchNumber, string entityName, int limit = int.MaxValue) where T : EntityPatchNote
        {
            var collection = _liteDB.GetCollection<T>();
            IEnumerable<T> results;
            results = collection.Find(Query.Or(Query.EQ("PatchNumber", patchNumber), Query.Contains("LocalName", entityName)), limit: limit);
            results.Concat(collection.Find(Query.Or(Query.EQ("PatchNumber", patchNumber), Query.Contains("RealName", entityName)), limit: limit));
            results.Concat(collection.Find(Query.Or(Query.EQ("PatchNumber", patchNumber), Query.Contains("InternalName", entityName)), limit: limit));
            results.Concat(collection.Find(Query.Or(Query.EQ("PatchNumber", patchNumber), Query.Contains("Aliases[*]", entityName)), limit: limit));
            return results.Take(limit);
        }

        public T GetEntityInfo<T>(int entityId) where T : EntityInfo
        {
            var collection = _liteDB.GetCollection<T>();
            var results = collection.FindOne(Query.EQ("EntityId", entityId));
            return results;
        }

        public IEnumerable<T> GetEntityInfo<T>(string entityName, int limit = int.MaxValue) where T : EntityInfo
        {
            var collection = _liteDB.GetCollection<T>();
            IEnumerable<T> results;
            results = collection.Find(Query.Contains("LocalName", entityName), limit: limit);
            results.Concat(collection.Find(Query.Contains("RealName", entityName), limit: limit));
            results.Concat(collection.Find(Query.Contains("InternalName", entityName), limit: limit));
            results.Concat(collection.Find(Query.Contains("Aliases[*]", entityName), limit: limit));
            return results.Take(limit);
        }
    }
}
