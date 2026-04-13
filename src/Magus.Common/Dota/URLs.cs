using Magus.Common.Dota.Enums;
using System.Text.RegularExpressions;

namespace Magus.Common.Dota;

public static class URLs
{
    public const string PatchSite = "https://www.dota2.com/patches/";
    public const string SteamCDN = "https://cdn.cloudflare.steamstatic.com";
    public const string DotaCdn = SteamCDN + "/apps/dota2/";
    public const string Hero = DotaCdn + "images/dota_react/heroes/";
    public const string HeroCrop = DotaCdn + "images/dota_react/heroes/crops/";
    public const string Ability = DotaCdn + "images/dota_react/abilities/";
    public const string Item = DotaCdn + "images/dota_react/items/";

    public const string StrengthIcon = DotaCdn + "images/dota_react/icons/hero_strength.png";
    public const string AgilityIcon = DotaCdn + "images/dota_react/icons/hero_agility.png";
    public const string IntelligenceIcon = DotaCdn + "images/dota_react/icons/hero_intelligence.png";
    public const string UniversalIcon = DotaCdn + "images/dota_react/icons/hero_universal.png";

    public const string DamageIcon = DotaCdn + "images/dota_react/heroes/stats/icon_damage.png";
    public const string AttackTimeIcon = DotaCdn + "images/dota_react/heroes/stats/icon_attack_time.png";
    public const string AttackRangeIcon = DotaCdn + "images/dota_react/heroes/stats/icon_attack_range.png";
    public const string ProjectileSpeedIcon = DotaCdn + "images/dota_react/heroes/stats/icon_projectile_speed.png";
    public const string ArmourIcon = DotaCdn + "images/dota_react/heroes/stats/icon_armor.png";
    public const string MagicResistIcon = DotaCdn + "images/dota_react/heroes/stats/icon_magic_resist.png";
    public const string MoveSpeedIcon = DotaCdn + "images/dota_react/heroes/stats/icon_movement_speed.png";
    public const string TurnRateIcon = DotaCdn + "images/dota_react/heroes/stats/icon_turn_rate.png";
    public const string VisionIcon = DotaCdn + "images/dota_react/heroes/stats/icon_vision.png";

    public const string DotaColourLogo = DotaCdn + "/images/dota_react/footer_logo.png";
    public const string DotaWhiteLogo = DotaCdn + "/images/dota_react/global/dota2_logo_horiz.png";

    public static string GetHeroUrl(string heroName)
        => $"https://www.dota2.com/hero/{Regex.Replace(heroName.ToLower(), @"[^a-zA-Z0-9-']", string.Empty)}";

    public static string GetHeroImage(string internalName)
        => $"{Hero}{internalName[14..]}.png"; // ignore "npc_dota_hero_" from internal name

    public static string GetAbilityImage(string internalName)
        => $"{Ability}{internalName}.png";

    public static string GetItemImage(string internalName)
        => $"{Item}{internalName[5..]}.png"; // ignore "item_" from internal name

    public static string GetTeamLogo(int teamId)
        => DotaCdn + $"/teamlogos/{teamId}.png";

    public static string GetAttributeIcon(this AttributePrimary attribute)
        => attribute switch
        {
            AttributePrimary.DOTA_ATTRIBUTE_STRENGTH  => StrengthIcon,
            AttributePrimary.DOTA_ATTRIBUTE_AGILITY   => AgilityIcon,
            AttributePrimary.DOTA_ATTRIBUTE_INTELLECT => IntelligenceIcon,
            AttributePrimary.DOTA_ATTRIBUTE_ALL       => UniversalIcon,
            _                                         => throw new ArgumentOutOfRangeException(nameof(attribute)),
        };
}
