using Magus.Common.Enums;
using Magus.Data.Enums;
using Magus.Data.Models;
using Magus.Data.Models.Discord;
using Magus.Data.Models.Dota;
using Magus.Data.Models.Embeds;
using Magus.Data.Models.Magus;
using System.Linq.Expressions;

namespace Magus.Data.Services;

public interface IAsyncDataService
{
    void CreateCollection<T>() where T : ISnowflakeId;
    Task DeleteCollection<T>() where T : ISnowflakeId;
    Task<bool> DeleteRecord<T>(ulong id) where T : ISnowflakeId;
    Task<string> EnsureIndex<T>(Expression<Func<T, object>> field, bool unique = false, bool caseSensitive = true) where T : ISnowflakeId;
    Task<T> GetEntityInfo<T>(int entityId, string locale = "en") where T : EntityInfoEmbed;
    Task<IEnumerable<T>> GetEntityInfo<T>(string entityName, string locale = "en", int limit = int.MaxValue) where T : EntityInfoEmbed;
    Task<IEnumerable<EntityLocalisation>> GetEntityLocalisations(EntityType type);
    Task<GeneralPatchNoteEmbed> GetGeneralPatchNote(string patchNumber, string locale = "en");
    Task<AbilityInfoEmbed> GetHeroScepter(int heroId, string locale = "en");
    Task<AbilityInfoEmbed> GetHeroShard(int heroId, string locale = "en");
    Task<Patch> GetLatestPatch();
    Task<Announcement?> GetLatestPublishedAnnouncement(Topic topic);
    Task<Patch> GetPatch(string patch);
    Task<IEnumerable<Patch>> GetPatches(string patch, int limit = int.MaxValue, bool orderByDesc = false);
    Task<T> GetPatchNote<T>(string patchNumber, int entityId, string locale = "en") where T : EntityPatchNoteEmbed;
    Task<T> GetPatchNote<T>(string patchNumber, string entityName, string locale = "en") where T : EntityPatchNoteEmbed;
    Task<IEnumerable<T>> GetPatchNotes<T>(int entityId, string locale = "en", int limit = int.MaxValue, bool orderByDesc = false) where T : EntityPatchNoteEmbed;
    Task<IEnumerable<T>> GetPatchNotes<T>(string entityName, string locale = "en", int limit = int.MaxValue, bool orderByDesc = false) where T : EntityPatchNoteEmbed;
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
