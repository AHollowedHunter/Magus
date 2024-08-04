using Magus.DotaParser.DotaFilePaths;
using Microsoft.Extensions.Options;
using SteamDatabase.ValvePak;
using ValveKeyValue;
using ValveResourceFormat;
using ValveResourceFormat.IO;

namespace Magus.DotaParser;

internal sealed class GameFileProvider : IDisposable
{
    private readonly KVSerializer _kvTextSerializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
    private readonly Package _pak01;

    private readonly string _rootGamePath;

    public GameFileProvider(IOptions<DotaParserConfig> config)
    {
        _rootGamePath = config.Value.GameFileLocation;

        _pak01 = ReadPak01();
    }

    public KVDocument GetPak01TextFile(string path, KVSerializerOptions? options = default)
    {
        options ??= KVSerializerOptions.DefaultOptions;

        var entry = _pak01.FindEntry(path) ?? throw new FileNotFoundException($"pak01 entry not found: {path}");

        _pak01.ReadEntry(entry, out byte[] entryData);

        ContentFile contentFile;
        using (var entryResource = new Resource())
        {
            using var entryStream = new MemoryStream(entryData);
            entryResource.Read(entryStream);
            contentFile = FileExtract.Extract(entryResource, null);
        }

        using var dataStream = new MemoryStream(contentFile.Data);
        return _kvTextSerializer.Deserialize(dataStream, options);
    }

    private Package ReadPak01()
    {
        var package = new Package();
        package.Read(Path.Combine(_rootGamePath, Pak01.FilePath));
        package.VerifyHashes();
        return package;
    }

    void IDisposable.Dispose()
    {
        _pak01.Dispose();
    }
}
