using Magus.Data.Enums;
using Magus.Data.Models.Dota;
using Meilisearch;
using Meilisearch.QueryParameters;

namespace Magus.Data.Services;

public sealed class MeilisearchService
{
    private readonly MeilisearchClient _client;

    public MeilisearchService(IHttpClientFactory httpClientFactory)
    {
        var httpClient = httpClientFactory.CreateClient("meilisearch");
        httpClient.BaseAddress = new Uri("http://localhost:7700"); // HACK testing
        _client = new MeilisearchClient(httpClient, "12345678");
    }

    #region Index

    /// <summary>
    /// Create a new index. If including a primary key, the index must not exist
    /// or it should have no documents within it.
    /// </summary>
    /// <exception cref="Exception"/>
    public async Task CreateIndexAsync(string indexUid, string? primaryKey = null, Settings? settings = null)
    {
        var createTask = await _client.CreateIndexAsync(indexUid, primaryKey).ConfigureAwait(false);
        await WaitForResultAsync(createTask).ConfigureAwait(false);

        if (settings is not null)
        {
            var index = _client.Index(indexUid);
            var updateTask = await index.UpdateSettingsAsync(settings).ConfigureAwait(false);
            await WaitForResultAsync(updateTask).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Deletes the Index
    /// </summary>
    /// <exception cref="Exception"/>
    public async Task DeleteIndexAsync(string indexUid)
    {
        var index = _client.Index(indexUid);
        var task = await index.DeleteAsync().ConfigureAwait(false);
        await WaitForResultAsync(task).ConfigureAwait(false);
    }

    /// <summary>
    /// Simple wrapper to get an existing index, or throw if it doesn't.
    /// </summary>
    /// <remarks>This is useful to ensure indexes are not created by accident,
    /// at a slight performance cost due to the http query.</remarks>
    /// <param name="indexUid"></param>
    /// <returns>The specified Index</returns>
    /// <exception cref="MeilisearchApiError">If the Index does not exist</exception>
    private async Task<Meilisearch.Index> GetIndexAsync(string indexUid)
        => await _client.GetIndexAsync(indexUid).ConfigureAwait(false);

    #endregion

    #region documents

    /// <summary>
    /// Add the documents to the specified index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="indexUid">Defaults to type name of <typeparamref name="T"/></param>
    /// <exception cref="Exception"/>
    public async Task AddDocumentsAsync<T>(IEnumerable<T> values, string? indexUid = null) where T : class
    {
        indexUid ??= typeof(T).Name;
        var index = _client.Index(indexUid);
        var task = await index.AddDocumentsAsync(values).ConfigureAwait(false);
        await WaitForResultAsync(task).ConfigureAwait(false);
    }

    public Task<EntityInfo> GetEntityInfoAsync(
        string internalName,
        string locale = "en",
        string indexUid = nameof(EntityInfo))
    {
        var documentId = EntityInfo.MakeUniqueId(internalName, locale);
        var index = _client.Index(indexUid);
        return index.GetDocumentAsync<EntityInfo>(documentId);
    }

    public async Task<IEnumerable<EntityInfo>> GetAllEntityInfoAsync(
        EntityType entityType = EntityType.None,
        string locale = "en",
        string indexUid = nameof(EntityInfo))
    {
        var index = _client.Index(indexUid);
        var filter = $"{nameof(EntityInfo.Locale)}='{locale}'";
        if (entityType is not EntityType.None)
            filter += $" AND {nameof(EntityInfo.EntityType)}='{entityType}'";

        return (await index.GetDocumentsAsync<EntityInfo>(new DocumentsQuery { Limit = int.MaxValue, Filter = filter })
                .ConfigureAwait(false))
            .Results;
    }

    #endregion

    #region search

    public async Task<IEnumerable<T>> SearchIndexAsync<T>(string? query, int limit = 25, string? indexUid = null)
        where T : class
    {
        indexUid ??= typeof(T).Name;
        var index = _client.Index(indexUid);

        var searchQuery = new SearchQuery() { Limit = limit, };
        var result = await index.SearchAsync<T>(query, searchQuery).ConfigureAwait(false);
        return result.Hits;
    }

    public async Task<IEnumerable<Entity>> SearchEntityAsync(
        string? query,
        EntityType type = EntityType.None,
        int limit = 25)
    {
        var index = _client.Index(nameof(Entity));

        var searchQuery = new SearchQuery() { Limit = limit, };

        if (type != EntityType.None)
            searchQuery.Filter = $"{nameof(Entity.EntityType)} = '{type}'";

        var result = await index.SearchAsync<Entity>(query, searchQuery).ConfigureAwait(false);
        return result.Hits;
    }

    public async Task<IEnumerable<Entity>> SearchEntityWithFiltersAsync(
        string? query,
        string[] filters,
        EntityType type = EntityType.None,
        int limit = 25)
    {
        var index = _client.Index(nameof(Entity));

        var searchQuery = new SearchQuery() { Limit = limit, };

        // TODO improve all this, check meilisearch C# docs
        string filter;
        if (filters.Length == 1)
            filter = "EntityFilters = '" + filters[0] + "'";
        else
            filter = $"EntityFilters = '{string.Join("' AND EntityFilters = '", filters)}'";

        if (type != EntityType.None)
            searchQuery.Filter = filter + $" AND {nameof(Entity.EntityType)} = '{type}'";

        // filter obj test
        // object objFilter;
        // if (filters.Length == 1)
        //     objFilter = new { EntityFilters = filters[0] };
        // else
        //     objFilter = new { EntityFilters = filters };
        // objFilter = $"EntityFilters = '{string.Join("' AND EntityFilters = '", filters)}'";
        //
        // if (type != EntityType.None)
        //     objFilter += $" AND {nameof(Entity.EntityType)} = '{type}'";
        // searchQuery.Filter = objFilter;


        //

        var result = await index.SearchAsync<Entity>(query, searchQuery).ConfigureAwait(false);
        return result.Hits;
    }

    public async Task<T?> SearchTopResultAsync<T>(string? query, string? indexUid = null) where T : class
        => (await SearchIndexAsync<T>(query, 1, indexUid).ConfigureAwait(false)).FirstOrDefault();

    public async Task<Entity?> SearchTopEntityAsync(string? query, EntityType entityType)
        => (await SearchEntityAsync(query, entityType, 1).ConfigureAwait(false)).FirstOrDefault();

    #endregion

    #region Patch

    public async Task<Patch> GetLatestPatchAsync()
    {
        var index = _client.Index(nameof(Patch));

        var searchQuery = new SearchQuery() { Limit = 1, Sort = [$"{nameof(Patch.Timestamp)}:desc"] };
        return (await index.SearchAsync<Patch>("", searchQuery).ConfigureAwait(false)).Hits.Single();
    }

    public async Task<IEnumerable<Patch>> GetPatchesAsync(string? query, int limit = 25, bool orderByDesc = true)
    {
        var index = _client.Index(nameof(Patch));

        var searchQuery = new SearchQuery()
        {
            Limit = limit, Sort = [$"{nameof(Patch.Timestamp)}:{(orderByDesc ? "desc" : "asc")}"]
        };
        return (await index.SearchAsync<Patch>(query, searchQuery).ConfigureAwait(false)).Hits;
    }

    public async Task<IEnumerable<PatchNote>> SearchPatchNotesAsync(
        string? query,
        string? patch = null,
        PatchNoteType? patchType = null,
        string locale = "en",
        int limit = 25,
        bool orderByDesc = true)
    {
        var index = _client.Index(nameof(PatchNote));

        var filter = $"{nameof(PatchNote.Locale)}='{locale}'";

        if (patch is not null)
            filter += $" AND {nameof(PatchNote.PatchNumber)}='{patch}'";
        if (patchType is not null)
            filter += $" AND {nameof(PatchNote.PatchNoteType)}='{patchType}'";

        var searchQuery = new SearchQuery() { Limit = limit, Filter = filter, };

        if (orderByDesc)
            searchQuery.Sort = [$"{nameof(PatchNote.Timestamp)}:desc"];
        else
            searchQuery.Sort = [$"{nameof(PatchNote.Timestamp)}:asc"];

        var result = await index.SearchAsync<PatchNote>(query, searchQuery).ConfigureAwait(false);
        return result.Hits;
    }

    #endregion

    #region private helpers

    /// <summary>
    /// Wait for the task to finish. Throws if failed.
    /// </summary>
    /// <exception cref="Exception">Throws an exception if the task failed, containing the TaskUid</exception>
    private async Task WaitForResultAsync(TaskInfo taskInfo)
    {
        var result = await _client.WaitForTaskAsync(taskInfo.TaskUid).ConfigureAwait(false);
        if (result.Status is TaskInfoStatus.Failed)
            throw new Exception(
                $"Task failed: {result.Uid}. Please check Meilisearch for specific error."); // TODO specific exception?
    }

    #endregion
}
