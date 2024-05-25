using Magus.Data.Models.Dota;
using Magus.Data.Services;
using Meilisearch;
using System.Diagnostics;
using ValveKeyValue;

namespace Magus.DataBuilder;

public sealed class PatchListUpdater
{
    private readonly ILogger<PatchNoteUpdater> _logger;
    private readonly MeilisearchService _meilisearchService;
    private readonly KVSerializer _kvSerializer;

    public PatchListUpdater(ILogger<PatchNoteUpdater> logger, MeilisearchService meilisearchService)
    {
        _logger = logger;
        _meilisearchService = meilisearchService;

        _kvSerializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
    }

    public async Task Update()
    {
        _logger.LogInformation("Starting Patch List Update");
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        await UpdatePatchList();

        stopwatch.Stop();
        var timeTaken = stopwatch.Elapsed.TotalSeconds;
        _logger.LogInformation("Finished Patch List Update");
        _logger.LogInformation("Time Taken: {0:0.#}s", timeTaken);
    }

    private async Task UpdatePatchList()
    {
        _logger.LogInformation("Getting Patches");
        var patchManifest = await _kvSerializer.GetKVObjectFromLocalUri(Dota2GameFiles.PatchNotes);

        List<Patch> patchList = [];

        foreach (var patch in patchManifest.Children)
            patchList.Add(CreatePatchInfo(patch));

        _logger.LogInformation("Finished getting patches");

        await StorePatchList(patchList);
    }

    private async Task StorePatchList(List<Patch> patchList)
    {
        _logger.LogInformation("Saving Patch List in database");

        string[] searchAndSortAttributes = [nameof(Patch.PatchNumber), nameof(Patch.Timestamp)];
        Settings settings = new()
        {
            SortableAttributes = searchAndSortAttributes,
            SearchableAttributes = searchAndSortAttributes,
        };
        try { await _meilisearchService.DeleteIndexAsync(nameof(Patch)); } catch { } // HACK for testing. Will use a swap index later.
        await _meilisearchService.CreateIndexAsync(nameof(Patch), nameof(Patch.UniqueId), settings);
        await _meilisearchService.AddDocumentsAsync(patchList);
    }

    private static Patch CreatePatchInfo(KVObject patch)
    {
        var patchNumber = patch.Children.First(x => x.Name == "patch_name").Value.ToString()!.Replace("patch ", "");
        return new(
            patchNumber.Replace('.', '-'),
            patchNumber,
            GetPatchTimestamp(patch));
    }

    private static ulong GetPatchTimestamp(KVObject patch)
        => (ulong)DateTimeOffset.Parse(patch.Children.First(x => x.Name == "patch_date").Value.ToString()!).ToUnixTimeSeconds();
}
