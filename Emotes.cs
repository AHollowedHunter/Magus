using Discord;
using Magus.Data.Models.Dota;

namespace Magus.Data
{
    public static class Emotes
    {
        // public static Emote Icon => Emote.Parse($"<:icon_:00000000>");
        public static Emote Spacer => Emote.Parse($"<:spacer:973678534637813760>");

        public static Emote StrengthIcon => Emote.Parse($"<:hero_strength:945826749118291998>");
        public static Emote AgilityIcon => Emote.Parse($"<:hero_agility:945826748539478027>");
        public static Emote IntelligenceIcon => Emote.Parse($"<:hero_intelligence:945826748845678592>");

        public static Emote ArmourIcon => Emote.Parse($"<:icon_armor:945826748921167873>");
        public static Emote AttackRangeIcon => Emote.Parse($"<:icon_attack_range:945826749084762112>");
        public static Emote AttackTimeIcon => Emote.Parse($"<:icon_attack_time:945826748996661358>");
        public static Emote DamageIcon => Emote.Parse($"<:icon_damage:945826748992479322>");
        public static Emote MagicResistIcon => Emote.Parse($"<:icon_magic_resist:945826749130883082>");
        public static Emote MoveSpeedIcon => Emote.Parse($"<:icon_movement_speed:945826749571293244>");
        public static Emote ProjectileSpeedIcon => Emote.Parse($"<:icon_projectile_speed:945826748992466954>");
        public static Emote TurnRateIcon => Emote.Parse($"<:icon_turn_rate:945826749009240064>");
        public static Emote VisionIcon => Emote.Parse($"<:icon_vision:945826748895985664>");

        public static Emote BreakIcon => Emote.Parse($"<:icon_break:945836553463296050>");
        public static Emote GoldIcon => Emote.Parse($"<:icon_gold:945836553027067964>");
        public static Emote IllusionIcon => Emote.Parse($"<:icon_illusion:945836553073213440>");
        public static Emote ManaIcon => Emote.Parse($"<:icon_mana:945836552838316165>");
        public static Emote BuffIcon => Emote.Parse($"<:icon_modifier_buff:945836553106755636>");
        public static Emote DebuffIcon => Emote.Parse($"<:icon_modifier_debuff:945836553052229632>");
        public static Emote ScepterIcon => Emote.Parse($"<:icon_scepter:945836553161293845>");
        public static Emote ScepterUpgradeIcon => Emote.Parse($"<:icon_scepter_upgade:945836553060638780>");
        public static Emote ShardIcon => Emote.Parse($"<:icon_shard:945836553584918528>");
        public static Emote ShardUpgradeIcon => Emote.Parse($"<:icon_shard_upgrade:945836553589112854>");
        public static Emote SpellImmuneIcon => Emote.Parse($"<:icon_spell_immunity_block:945836553396178975>");
        public static Emote TalentTreeIcon => Emote.Parse($"<:icon_talent_tree:945836553266147369>");
        public static Emote RangedIcon => Emote.Parse($"<:icon_ranged:946541321961742388>");
        public static Emote MeleeIcon => Emote.Parse($"<:icon_melee:946541322188238938>");
        public static Emote CooldownIcon => Emote.Parse($"<:icon_cooldown:945836553756880967>");

        public static Emote GetAttributeEmote(this AttributePrimary attribute)
        {
            switch (attribute)
            {
                case AttributePrimary.DOTA_ATTRIBUTE_STRENGTH:
                    return StrengthIcon;
                case AttributePrimary.DOTA_ATTRIBUTE_AGILITY:
                    return AgilityIcon;
                case AttributePrimary.DOTA_ATTRIBUTE_INTELLECT:
                    return IntelligenceIcon;
                default:
                    return null;
            }
        }

        public static Emote GetAttackTypeIcon(this AttackCapabilities attackType)
            => attackType == AttackCapabilities.DOTA_UNIT_CAP_MELEE_ATTACK ? MeleeIcon : RangedIcon; //todo adjust this
    }
}
