using Magus.Data.Enums;
using Magus.Data.Models.V2;
using Meilisearch;

namespace Magus.Data.Services;

public sealed class MeilisearchService
{
    private readonly MeilisearchClient _client;

    public MeilisearchService()
    {
        _client = new MeilisearchClient("http://localhost:7700", "12345678"); // HACK testing
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
        var index = await GetIndexAsync(indexUid).ConfigureAwait(false);
        var task = await index.AddDocumentsAsync(values).ConfigureAwait(false);
        await WaitForResultAsync(task).ConfigureAwait(false);
    }
    #endregion

    #region search
    public async Task<IEnumerable<T>> SearchIndexAsync<T>(string? query, int limit = 25, string? indexUid = null) where T : class
    {
        indexUid ??= typeof(T).Name;
        var index = await GetIndexAsync(indexUid).ConfigureAwait(false);

        var searchQuery = new SearchQuery()
        {
            Limit = limit,
        };
        var result = await index.SearchAsync<T>(query, searchQuery).ConfigureAwait(false);
        return result.Hits;
    }

    public async Task<IEnumerable<Entity>> SearchEntityMetaAsync(string? query, EntityType type = EntityType.None, int limit = 25)
    {
        var index = await GetIndexAsync(nameof(Entity)).ConfigureAwait(false);

        var searchQuery = new SearchQuery() { Limit = limit, };

        if (type != EntityType.None)
            searchQuery.Filter = $"{nameof(Entity.EntityType)} = '{type}'";

        var result = await index.SearchAsync<Entity>(query, searchQuery).ConfigureAwait(false);
        return result.Hits;
    }

    public async Task<T?> SearchTopResultAsync<T>(string? query, string? indexUid = null) where T : class
    {
        indexUid ??= typeof(T).Name;
        var index = await GetIndexAsync(indexUid).ConfigureAwait(false);

        var searchQuery = new SearchQuery() { Limit = 1, };

        var result = await index.SearchAsync<T>(query, searchQuery).ConfigureAwait(false);
        return result.Hits.SingleOrDefault();
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
            throw new Exception($"Task failed: {result.Uid}. Please check Meilisearch for specific error."); // TODO specific exception?
    }
    #endregion
}
