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

        public void CreateCollection<T>() where T : ISnowflakeRecord
        {
            throw new NotImplementedException("LiteDB creates a collection when inserting a record");
        }

        public bool DeleteCollection<T>() where T : ISnowflakeRecord
        {
            return _liteDB.DropCollection(typeof(T).Name);
        }

        public void InsertRecord<T>(T record) where T : ISnowflakeRecord
        {
            var collection = _liteDB.GetCollection<T>();
            collection.Insert(record);
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

        public Models.Embeds.GeneralPatchNote GetGeneralPatchNote(string patchNumber)
        {
            var collection = _liteDB.GetCollection<Models.Embeds.GeneralPatchNote>();
            var results = collection.FindOne(Query.EQ("PatchNumber", patchNumber));
            return results;
        }

        public IEnumerable<T> GetPatchNotes<T>(int entityId, int limit = int.MaxValue, bool orderByDesc = false) where T : EntityPatchNote
        {
            var collection = _liteDB.GetCollection<T>();
            var results = collection.Find(Query.EQ("EntityId", entityId));
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

        public IEnumerable<T> GetPatchNotes<T>(string entityName, int limit = int.MaxValue, bool orderByDesc = false) where T : EntityPatchNote
        {
            var collection = _liteDB.GetCollection<T>();
            IEnumerable<T> results;
            results = collection.Find(Query.Contains("LocalName", entityName), limit: limit);
            results.Concat(collection.Find(Query.Contains("RealName", entityName), limit: limit));
            results.Concat(collection.Find(Query.Contains("InternalName", entityName), limit: limit));
            results.Concat(collection.Find(Query.Contains("Aliases[*]", entityName), limit: limit));
            return results.Take(limit);
        }

        public T GetPatchNote<T>(string patchNumber, int entityId) where T : EntityPatchNote
        {
            var collection = _liteDB.GetCollection<T>();
            var result = collection.FindOne(Query.And(Query.EQ("PatchNumber", patchNumber), Query.EQ("EntityId", entityId)));
            return result;
        }

        public IEnumerable<T> GetPatchNote<T>(string patchNumber, string entityName, int limit = int.MaxValue) where T : EntityPatchNote
        {
            var collection = _liteDB.GetCollection<T>();
            IEnumerable<T> results;
            results = collection.Find(Query.And(Query.EQ("PatchNumber", patchNumber), Query.Contains("LocalName", entityName)), limit: limit);
            results.Concat(collection.Find(Query.And(Query.EQ("PatchNumber", patchNumber), Query.Contains("RealName", entityName)), limit: limit));
            results.Concat(collection.Find(Query.And(Query.EQ("PatchNumber", patchNumber), Query.Contains("InternalName", entityName)), limit: limit));
            results.Concat(collection.Find(Query.And(Query.EQ("PatchNumber", patchNumber), Query.Contains("Aliases[*]", entityName)), limit: limit));
            return results.Take(limit);
        }

        public T GetEntityInfo<T>(int entityId) where T : EntityInfo
        {
            var collection = _liteDB.GetCollection<T>();
            var result = collection.FindById(entityId);
            return result;
        }

        public IEnumerable<T> GetEntityInfo<T>(string entityName, int limit = int.MaxValue) where T : EntityInfo
        {
            var collection = _liteDB.GetCollection<T>();
            IEnumerable<T> results;
            results = collection.Find(Query.Contains("LocalName", entityName), limit: limit); // todo startswith?
            results.Concat(collection.Find(Query.Contains("RealName", entityName), limit: limit));
            results.Concat(collection.Find(Query.Contains("InternalName", entityName), limit: limit));
            //results.Concat(collection.Find(Query.Contains("Aliases[*]", entityName), limit: limit));
            return results.Take(limit);
        }

        public bool AddIndex<T>(string fieldName, bool unique = false) where T : ISnowflakeRecord
        {
            var collection = _liteDB.GetCollection<T>();
            if (collection == null)
                return false;
            return collection.EnsureIndex(fieldName, unique);
        }
    }
}
