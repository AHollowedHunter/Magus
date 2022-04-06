using Magus.Data.Models;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;

namespace Magus.Data
{
    public interface IDatabaseService : IDisposable
    {
        void CreateCollection<T>() where T : IGuidRecord, ISnowflakeRecord;

        /// <summary>
        /// Delete the collection associated with the type given
        /// </summary>
        /// <returns>A bool indicating if deleting was successful</returns>
        bool DeleteCollection<T>() where T : IGuidRecord, ISnowflakeRecord;

        /// <summary>
        /// Insert a given record, and return the record as stored
        /// </summary>
        T InsertRecord<T>(T record) where T : IGuidRecord, ISnowflakeRecord;

        /// <summary>
        /// Update a given record, and return the record as stored
        /// </summary>
        T UpdateRecord<T>(T record) where T : IGuidRecord, ISnowflakeRecord;

        /// <summary>
        /// Attempts to deletes the given record
        /// </summary>
        /// <returns>A bool indicating if deleting was successful</returns>
        bool DeleteRecord<T>(T record) where T : IGuidRecord, ISnowflakeRecord;

        /// <summary>
        /// Get record via an Id
        /// </summary>
        /// <typeparam name="T">Type of record desired</typeparam>
        /// <param name="id">Specific Id</param>
        T GetRecord<T>(Guid id) where T : IGuidRecord;
        /// <summary>
        /// Get record via an Id
        /// </summary>
        /// <typeparam name="T">Type of record desired</typeparam>
        /// <param name="id">Specific Id</param>
        /// <returns></returns>
        T GetRecord<T>(ulong id) where T : ISnowflakeRecord;
        /// <summary>
        /// Get a collection of records
        /// </summary>
        /// <typeparam name="T">Type of record desired</typeparam>
        /// <param name="limit">How many records to return</param>
        /// <param name="orderByDesc">Order using Id ascending if false, or descending if true.</param>
        IEnumerable<T> GetRecords<T>(int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeRecord;
        /// <summary>
        /// Get a collection of records starting at the specified Id
        /// </summary>
        /// <typeparam name="T">Type of record desired</typeparam>
        /// <param name="id">Starting Id position to retrieve.</param>
        /// <param name="limit">How many records to return.</param>
        /// <param name="orderByDesc">Order using Id ascending if false, or descending if true.</param>
        /// <returns></returns>
        IEnumerable<T> GetRecords<T>(ulong id, int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeRecord;

        /// <summary>
        /// Get the latest patch metadata
        /// </summary>
        /// <returns>A Patch object</returns>
        Patch GetLatestPatch();
        /// <summary>
        /// Get the specified patch metadata
        /// </summary>
        Patch GetPatch(string patch);
        /// <summary>
        /// Get a collection of patches starting at the specified patch number
        /// </summary>
        IEnumerable<Patch> GetPatches(string patch, int limit = int.MaxValue, bool orderByDesc = false);

        /// <summary>
        /// Get the General notes for the specified patch
        /// </summary>
        GeneralPatchNote GetGeneralPatchNote(string patchNumber);
        /// <summary>
        /// Get the specified types patch note(s) for the specified entityId
        /// </summary>
        IEnumerable<T> GetPatchNotes<T>(int entityId, int limit = int.MaxValue) where T : EntityPatchNote;

        /// <summary>
        /// Get the specified types patch note(s) for the specified entityName
        /// </summary>
        IEnumerable<T> GetPatchNotes<T>(string entityName, int limit = int.MaxValue) where T : EntityPatchNote;
        /// <summary>
        /// Get the specified types patch note(s) for the specified patch and entityId
        /// </summary>
        IEnumerable<T> GetPatchNotes<T>(string patchNumber, int entityId, int limit = int.MaxValue) where T : EntityPatchNote;
        /// <summary>
        /// Get the specified types patch note(s) for the specified patch and entity name
        /// </summary>
        IEnumerable<T> GetPatchNotes<T>(string patchNumber, string entityName, int limit = int.MaxValue) where T : EntityPatchNote;
        
        /// <summary>
        /// Get the specified types info via an entityId
        /// </summary>
        IEnumerable<T> GetEntityInfo<T>(int entityId, int limit = int.MaxValue) where T : EntityInfo;
        /// <summary>
        /// Get the specified types info via an entityName
        /// </summary>
        IEnumerable<T> GetEntityInfo<T>(string entityName, int limit = int.MaxValue) where T : EntityInfo;
    }
}