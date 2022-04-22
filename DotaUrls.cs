using Magus.Data.Models.Dota;

namespace Magus.Data
{
    public static class DotaUrls
    {
        public static string BaseUrl => "https://cdn.cloudflare.steamstatic.com/apps/dota2/";
        public static string Hero => BaseUrl + "images/dota_react/heroes/";
        public static string HeroCrop => BaseUrl + "images/dota_react/heroes/crops/";
        public static string Ability => BaseUrl + "images/dota_react/abilities/";
        public static string Item => BaseUrl + "images/dota_react/items/";

        public static string StrengthIcon => BaseUrl + "images/dota_react/icons/hero_strength.png";
        public static string AgilityIcon => BaseUrl + "images/dota_react/icons/hero_agility.png";
        public static string IntelligenceIcon => BaseUrl + "images/dota_react/icons/hero_intelligence.png";

        public static string DamageIcon => BaseUrl + "images/dota_react/heroes/stats/icon_damage.png";
        public static string AttackTimeIcon => BaseUrl + "images/dota_react/heroes/stats/icon_attack_time.png";
        public static string AttackRangeIcon => BaseUrl + "images/dota_react/heroes/stats/icon_attack_range.png";
        public static string ProjectileSpeedIcon => BaseUrl + "images/dota_react/heroes/stats/icon_projectile_speed.png";
        public static string ArmourIcon => BaseUrl + "images/dota_react/heroes/stats/icon_armor.png";
        public static string MagicResistIcon => BaseUrl + "images/dota_react/heroes/stats/icon_magic_resist.png";
        public static string MoveSpeedIcon => BaseUrl + "images/dota_react/heroes/stats/icon_movement_speed.png";
        public static string TurnRateIcon => BaseUrl + "images/dota_react/heroes/stats/icon_turn_rate.png";
        public static string VisionIcon => BaseUrl + "images/dota_react/heroes/stats/icon_vision.png";

        public static string GetAttributeIcon(this PrimaryAttribute attribute)
        {
            switch (attribute)
            {
                case PrimaryAttribute.Strength:
                    return StrengthIcon;
                case PrimaryAttribute.Agility:
                    return AgilityIcon;
                case PrimaryAttribute.Intelligence:
                    return IntelligenceIcon;
                default:
                    return null;
            }
        }
    }
}
