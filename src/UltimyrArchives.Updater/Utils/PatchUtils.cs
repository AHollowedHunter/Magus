using UltimyrArchives.Updater.Extensions;

namespace UltimyrArchives.Updater.Utils;

public static class PatchUtils
{
    /// <summary>
    /// Assumes string is from PST, Valve HQ.
    /// </summary>
    public static long GetPatchTimestamp(KVObject patchObject)
    {
        var patchDate = patchObject.GetRequiredString("patch_date", CultureInfo.InvariantCulture);
        return DateTimeOffset.Parse(patchDate + " -08:00").ToUnixTimeSeconds();
    }
}
