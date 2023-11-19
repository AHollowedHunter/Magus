using Magus.Data;
using Magus.Data.Models.Dota;
using System.Diagnostics;
using ValveKeyValue;

namespace Magus.DataBuilder;

public class PatchListUpdater
{
    private readonly IAsyncDataService _db;
    private readonly ILogger<PatchNoteUpdater> _logger;
    private readonly KVSerializer _kvSerializer;

    public PatchListUpdater(IAsyncDataService db, ILogger<PatchNoteUpdater> logger)
    {
        _db = db;
        _logger = logger;

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

        List<Patch> patchList = new();

        foreach (var patch in patchManifest.Children)
            patchList.Add(CreatePatchInfo(patch));

        _logger.LogInformation("Finished getting patches");

        await StorePatchList(patchList);
    }

    private async Task StorePatchList(List<Patch> patchList)
    {
        _logger.LogInformation("Saving Patch List in database");
        _db.CreateCollection<Patch>();
        await _db.EnsureIndex<Patch>(x => x.PatchNumber, true, caseSensitive: false);
        await _db.EnsureIndex<Patch>(x => x.Timestamp, true);
        await _db.UpsertRecords(patchList);
    }

    private Patch CreatePatchInfo(KVObject patch)
       => new()
       {
           Id = GetPatchTimestamp(patch),
           PatchNumber = patch.Children.First(x => x.Name == "patch_name").Value.ToString()!.Replace("patch ", ""),
           Timestamp = GetPatchTimestamp(patch),
       };

    private ulong GetPatchTimestamp(KVObject patch)
        => (ulong)DateTimeOffset.Parse(patch.Children.First(x => x.Name == "patch_date").Value.ToString()!).ToUnixTimeSeconds();
}
