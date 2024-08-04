using Magus.Data.Models.Dota;
using Magus.Data.Services;
using Meilisearch;
using Microsoft.Extensions.Logging;

namespace UltimyrArchives.Updater.Services;

public class StorageService
{
    private const string TempIndexPostfix = "_TEMP";
    private const string PatchTempIndex = nameof(Patch) + TempIndexPostfix;

    private readonly ILogger<StorageService> _logger;
    private readonly MeilisearchService _meilisearchService;

    public StorageService(ILogger<StorageService> logger, MeilisearchService meilisearchService)
    {
        _logger             = logger;
        _meilisearchService = meilisearchService;
    }

    public async Task CleanTempIndexesAsync()
    {
        // TODO temp method, evaluate best approach
        await _meilisearchService.DeleteIndexAsync(PatchTempIndex);
    }

    public async Task StorePatchListTempAsync(List<Patch> patchList)
    {
        _logger.LogInformation("Saving Patch List in temporary index.");

        string[] searchAndSortAttributes = [nameof(Patch.PatchNumber), nameof(Patch.Timestamp)];
        Settings settings                = new() { SortableAttributes = searchAndSortAttributes, SearchableAttributes = searchAndSortAttributes, };


        await _meilisearchService.CreateIndexAsync(PatchTempIndex, nameof(Patch.UniqueId), settings);
        await _meilisearchService.AddDocumentsAsync(patchList, PatchTempIndex);
    }

    public async Task StorePatchNotesTempAsync(List<PatchNote> patchNotes)
    {
        throw new NotImplementedException();
    }

    public async Task StoreEntityTempAsync(List<Entity> entities)
    {
        throw new NotImplementedException();
    }

    public async Task StoreEntityInfoTempAsync(List<EntityInfo> entityInfos)
    {
        throw new NotImplementedException();
    }

    public async Task SwapAllTempIndexesAsync()
    {
        (string, string)[] indexes =
        [
            (nameof(Patch), PatchTempIndex),
        ];
        // TODO other indexes...

        _logger.LogInformation("Swapping all temporary indexes.");
        await _meilisearchService.SwapIndexes(indexes);
    }
}
