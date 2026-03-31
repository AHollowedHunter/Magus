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
    
    /// <summary>
    /// Only include the patch number i.e. 'patch 7.37' => '7.37'
    /// </summary>
    public static string GetPatchNumber(KVObject patchObject)
    {
        return patchObject.GetRequiredString("patch_name", CultureInfo.InvariantCulture)[6..];
    }
}
