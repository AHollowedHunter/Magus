using Microsoft.Extensions.Options;
using SteamDatabase.ValvePak;
using UltimyrArchives.Updater.DotaFilePaths;
using ValveKeyValue;
using ValveResourceFormat;
using ValveResourceFormat.IO;

namespace UltimyrArchives.Updater.Utils;

internal sealed class GameFileProvider : IDisposable
{
    private readonly KVSerializer _kvTextSerializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
    private readonly Package _pak01;

    private readonly string _rootGamePath;

    public GameFileProvider(IOptions<UpdaterConfig> config)
    {
        _rootGamePath = config.Value.GameFileLocation;

        _pak01 = ReadPackage(Pak01.FilePath);
    }

    public KVDocument GetPak01TextFile(string path, KVSerializerOptions? options = default)
    {
        options ??= KVSerializerOptions.DefaultOptions;

        var entryBytes = GetEntryBytes(path, _pak01);

        byte[] entryData;
        // .txt files don't need any special manipulation, just load the bytes.
        if (path.EndsWith(".txt"))
        {
            entryData = entryBytes;
        }
        else
        {
            using var entryResource = new Resource();
            using var entryStream   = new MemoryStream(entryBytes);
            entryResource.Read(entryStream);
            using var contentFile = FileExtract.Extract(entryResource, null);
            entryData = contentFile.Data;
        }

        using var dataStream = new MemoryStream(entryData);
        return _kvTextSerializer.Deserialize(dataStream, options);
    }

    public string GetPak01FileChecksum(string path)
        => GetEntry(path, _pak01).CRC32.ToString("X");

    private static PackageEntry GetEntry(string path, Package package)
        => package.FindEntry(path) ?? throw new FileNotFoundException($"Entry path '{path}' not found in package '{package.FileName}'.");

    private static byte[] GetEntryBytes(string path, Package package)
    {
        var entry = GetEntry(path, package);
        package.ReadEntry(entry, out byte[] entryData);
        return entryData;
    }

    private Package ReadPackage(string path)
    {
        var package = new Package();
        package.Read(Path.Combine(_rootGamePath, path));
        package.VerifyHashes();
        return package;
    }

    void IDisposable.Dispose()
    {
        _pak01.Dispose();
    }
}
