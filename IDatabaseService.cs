using Magus.Data.Models;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;

namespace Magus.Data
{
    public interface IDatabaseService : IDisposable
    {
        public const string DEFAULT_LOCALE = "en-GB";

        void CreateCollection<T>() where T : ISnowflakeRecord;

        /// <summary>
        /// Delete the collection associated with the type given
        /// </summary>
        /// <returns>A bool indicating if deleting was successful</returns>
        bool DeleteCollection<T>() where T : ISnowflakeRecord;

        /// <summary>
        /// Insert a given record
        /// </summary>
        ulong InsertRecord<T>(T record) where T : ISnowflakeRecord;

        /// <summary>
        /// Insert a collection of records, and return an int
        /// </summary>
        int InsertRecords<T>(IEnumerable<T> records) where T : ISnowflakeRecord;

        /// <summary>
        /// Update a given record, and return true is successful
        /// </summary>
        bool UpdateRecord<T>(T record) where T : ISnowflakeRecord;

        /// <summary>
        /// Update a collection of records, and return an int
        /// </summary>
        int UpdateRecords<T>(IEnumerable<T> records) where T : ISnowflakeRecord;

        /// <summary>
        /// Update or insert a given record, and return true is successful
        /// </summary>
        bool UpsertRecord<T>(T record) where T : ISnowflakeRecord;

        /// <summary>
        /// Update or insert a collection of records, and return an int
        /// </summary>
        int UpsertRecords<T>(IEnumerable<T> records) where T : ISnowflakeRecord;

        /// <summary>
        /// Attempts to deletes the given record type via the Id
        /// </summary>
        /// <returns>A bool indicating if deleting was successful</returns>
        bool DeleteRecord<T>(ulong id) where T : ISnowflakeRecord;

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
        GeneralPatchNoteEmbed GetGeneralPatchNote(string patchNumber, string locale = DEFAULT_LOCALE);
        /// <summary>
        /// Get the specified types patch note(s) for the specified entityId
        /// </summary>
        IEnumerable<T> GetPatchNotes<T>(int entityId, string locale = DEFAULT_LOCALE, int limit = int.MaxValue, bool orderByDesc = false) where T : EntityPatchNoteEmbed;

        /// <summary>
        /// Get the specified types patch note(s) for the specified entityName
        /// </summary>
        IEnumerable<T> GetPatchNotes<T>(string entityName, int limit = int.MaxValue, bool orderByDesc = false) where T : EntityPatchNoteEmbed;
        /// <summary>
        /// Get the specified types patch note for the specified patch and entityId
        /// </summary>
        T GetPatchNote<T>(string patchNumber, int entityId) where T : EntityPatchNoteEmbed;
        /// <summary>
        /// Get the specified types patch note for the specified patch and entity name
        /// </summary>
        //T GetPatchNote<T>(string patchNumber, string entityName) where T : EntityPatchNote;

        /// <summary>
        /// Get the specified types info via an entityId
        /// </summary>
        T GetEntityInfo<T>(int entityId) where T : EntityInfo;
        /// <summary>
        /// Get the specified types info via an entityName
        /// </summary>
        IEnumerable<T> GetEntityInfo<T>(string entityName, int limit = int.MaxValue) where T : EntityInfo;

        /// <summary>
        /// Create a index on the required field
        /// </summary>
        /// <param name="fieldName">Field to add as an index</param>
        /// <param name="unique">Whether the index should be enforced unique</param>
        /// <returns>True if the index was created, false if already exists</returns>
        bool EnsureIndex<T>(string fieldName, bool unique = false) where T : ISnowflakeRecord;
    }
}