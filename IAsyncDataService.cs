using Magus.Data.Models;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using System.Linq.Expressions;

namespace Magus.Data
{
    public interface IAsyncDataService
    {
        void CreateCollection<T>() where T : ISnowflakeRecord;
        Task DeleteCollection<T>() where T : ISnowflakeRecord;
        Task<bool> DeleteRecord<T>(ulong id) where T : ISnowflakeRecord;
        Task<string> EnsureIndex<T>(Expression<Func<T, object>> field, bool unique = false, bool caseSensitive = true) where T : ISnowflakeRecord;
        Task<T> GetEntityInfo<T>(int entityId, string locale = "en-GB") where T : EntityInfoEmbed;
        Task<IEnumerable<T>> GetEntityInfo<T>(string entityName, string locale = "en-GB", int limit = int.MaxValue) where T : EntityInfoEmbed;
        Task<GeneralPatchNoteEmbed> GetGeneralPatchNote(string patchNumber, string locale = "en-GB");
        Task<Patch> GetLatestPatch();
        Task<Patch> GetPatch(string patch);
        Task<IEnumerable<Patch>> GetPatches(string patch, int limit = int.MaxValue, bool orderByDesc = false);
        Task<T> GetPatchNote<T>(string patchNumber, int entityId, string locale = "en-GB") where T : EntityPatchNoteEmbed;
        Task<T> GetPatchNote<T>(string patchNumber, string entityName, string locale = "en-GB") where T : EntityPatchNoteEmbed;
        Task<IEnumerable<T>> GetPatchNotes<T>(int entityId, string locale = "en-GB", int limit = int.MaxValue, bool orderByDesc = false) where T : EntityPatchNoteEmbed;
        Task<IEnumerable<T>> GetPatchNotes<T>(string entityName, string locale = "en-GB", int limit = int.MaxValue, bool orderByDesc = false) where T : EntityPatchNoteEmbed;
        Task<T> GetRecord<T>(ulong id) where T : ISnowflakeRecord;
        Task<IEnumerable<T>> GetRecords<T>(int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeRecord;
        Task<IEnumerable<T>> GetRecords<T>(string locale = "en-GB", int limit = int.MaxValue, bool orderByDesc = false) where T : ISnowflakeRecord, ILocaleRecord;
        Task InsertRecord<T>(T record) where T : ISnowflakeRecord;
        Task InsertRecords<T>(IEnumerable<T> records) where T : ISnowflakeRecord;
        Task ReplaceRecord<T>(T record) where T : ISnowflakeRecord;
        Task ReplaceRecords<T>(IEnumerable<T> records) where T : ISnowflakeRecord;
        Task UpdateRecord<T>(T record) where T : ISnowflakeRecord;
        Task UpdateRecords<T>(IEnumerable<T> records) where T : ISnowflakeRecord;
        Task UpsertRecord<T>(T record) where T : ISnowflakeRecord;
        Task UpsertRecords<T>(IEnumerable<T> records) where T : ISnowflakeRecord;
    }
}