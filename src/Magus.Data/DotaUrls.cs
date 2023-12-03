using Magus.Data.Models.Dota;
using System.Text.RegularExpressions;

namespace Magus.Data;

public static class DotaUrls
{
    public static string BaseUrl  => "https://cdn.cloudflare.steamstatic.com/apps/dota2/";
    public static string Hero     => BaseUrl + "images/dota_react/heroes/";
    public static string HeroCrop => BaseUrl + "images/dota_react/heroes/crops/";
    public static string Ability  => BaseUrl + "images/dota_react/abilities/";
    public static string Item     => BaseUrl + "images/dota_react/items/";

    public static string StrengthIcon     => BaseUrl + "images/dota_react/icons/hero_strength.png";
    public static string AgilityIcon      => BaseUrl + "images/dota_react/icons/hero_agility.png";
    public static string IntelligenceIcon => BaseUrl + "images/dota_react/icons/hero_intelligence.png";
    public static string UniversalIcon    => BaseUrl + "images/dota_react/icons/hero_universal.png";

    public static string DamageIcon          => BaseUrl + "images/dota_react/heroes/stats/icon_damage.png";
    public static string AttackTimeIcon      => BaseUrl + "images/dota_react/heroes/stats/icon_attack_time.png";
    public static string AttackRangeIcon     => BaseUrl + "images/dota_react/heroes/stats/icon_attack_range.png";
    public static string ProjectileSpeedIcon => BaseUrl + "images/dota_react/heroes/stats/icon_projectile_speed.png";
    public static string ArmourIcon          => BaseUrl + "images/dota_react/heroes/stats/icon_armor.png";
    public static string MagicResistIcon     => BaseUrl + "images/dota_react/heroes/stats/icon_magic_resist.png";
    public static string MoveSpeedIcon       => BaseUrl + "images/dota_react/heroes/stats/icon_movement_speed.png";
    public static string TurnRateIcon        => BaseUrl + "images/dota_react/heroes/stats/icon_turn_rate.png";
    public static string VisionIcon          => BaseUrl + "images/dota_react/heroes/stats/icon_vision.png";

    public static string DotaColourLogo => "https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/footer_logo.png";
    public static string DotaWhiteLogo  => "https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/global/dota2_logo_horiz.png";

    public static string GetHeroUrl(string heroName)          => $"https://www.dota2.com/hero/{Regex.Replace(heroName.ToLower(), @"[^a-zA-Z0-9-']", string.Empty)}";
    public static string GetHeroImage(string internalName)    => $"{Hero}{internalName[14..]}.png"; // ignore "npc_dota_hero_" from internal name
    public static string GetAbilityImage(string internalName) => $"{Ability}{internalName}.png";
    public static string GetItemImage(string internalName)    => $"{Item}{internalName[5..]}.png"; // ignore "item_" from internal name

    public static string GetTeamLogo(int teamId) => $"https://cdn.cloudflare.steamstatic.com/apps/dota2/teamlogos/{teamId}.png";

    public static string GetAttributeIcon(this AttributePrimary attribute)
#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
     => attribute switch
     {
         AttributePrimary.DOTA_ATTRIBUTE_STRENGTH  => StrengthIcon,
         AttributePrimary.DOTA_ATTRIBUTE_AGILITY   => AgilityIcon,
         AttributePrimary.DOTA_ATTRIBUTE_INTELLECT => IntelligenceIcon,
         AttributePrimary.DOTA_ATTRIBUTE_ALL       => UniversalIcon,
     };
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

}
