using Meilisearch;

namespace Magus.Data.Services;

public sealed class MeilisearchService
{
    private MeilisearchClient _client;

    public MeilisearchService()
    {
        _client = new MeilisearchClient("http://localhost:7700", "12345678"); // HACK testing
    }

    public async Task CreateIndex<T>(string? primaryKey = null, IEnumerable<string>? searchableAttributes = null) where T : class
    {
        var indexName = typeof(T).Name;

        await _client.DeleteIndexAsync(indexName); // HACK temp for testing

        var createIndexTask = await _client.CreateIndexAsync(indexName, primaryKey);

        var index = _client.Index(indexName);

        if (searchableAttributes is not null)
        {
            var saTask = await index.UpdateSearchableAttributesAsync(searchableAttributes);
        }

        // TODO handle task response. Return something?
    }

    public async Task<int> AddDocuments<T>(IEnumerable<T> values) where T : class
    {
        var index = _client.Index(typeof(T).Name);
        var task = await index.AddDocumentsAsync(values);

        // TODO handle task response

        return task.TaskUid;
    }
}
