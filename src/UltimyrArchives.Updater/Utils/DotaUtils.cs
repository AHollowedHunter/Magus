namespace UltimyrArchives.Updater.Utils;

public static class DotaUtils
{
    /// <summary>
    /// Assumes string is from PST, Valve HQ.
    /// </summary>
    public static long GetPatchTimestamp(KVObject patchObject)
    {
        var patchDate = patchObject["patch_date"].ToString(CultureInfo.InvariantCulture);
        return DateTimeOffset.Parse(patchDate + " -08:00").ToUnixTimeSeconds();
    }
    
    /// <summary>
    /// Only include the patch number i.e. 'patch 7.37' => '7.37'
    /// </summary>
    public static string GetPatchNumber(KVObject patchObject)
    {
        return patchObject["patch_name"].ToString(CultureInfo.InvariantCulture)[6..];
    }

    /// <summary>
    /// Get hero name after 'npc_dota_hero_' i.e. npc_dota_hero_alchemist => alchemist
    /// </summary>
    public static string GetShortHeroName(string internalName)
        => internalName[14..];
}
