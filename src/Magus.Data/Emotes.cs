﻿using Discord;
using Magus.Data.Models.Dota;

namespace Magus.Data
{
    public static class Emotes
    {
        // public static Emote Icon => Emote.Parse($"<::00000000>");
        public static Emote Spacer => Emote.Parse($"<:spacer:973678534637813760>");

        public static Emote StrengthIcon     => Emote.Parse($"<:strength:945826749118291998>");
        public static Emote AgilityIcon      => Emote.Parse($"<:agility:945826748539478027>");
        public static Emote IntelligenceIcon => Emote.Parse($"<:intelligence:945826748845678592>");

        public static Emote ArmourIcon          => Emote.Parse($"<:armor:945826748921167873>");
        public static Emote AttackRangeIcon     => Emote.Parse($"<:attack_range:945826749084762112>");
        public static Emote AttackTimeIcon      => Emote.Parse($"<:attack_time:945826748996661358>");
        public static Emote DamageIcon          => Emote.Parse($"<:damage:945826748992479322>");
        public static Emote MagicResistIcon     => Emote.Parse($"<:magic_resist:945826749130883082>");
        public static Emote MoveSpeedIcon       => Emote.Parse($"<:movement_speed:945826749571293244>");
        public static Emote ProjectileSpeedIcon => Emote.Parse($"<:projectile_speed:945826748992466954>");
        public static Emote TurnRateIcon        => Emote.Parse($"<:turn_rate:945826749009240064>");
        public static Emote VisionIcon          => Emote.Parse($"<:vision:945826748895985664>");

        public static Emote BreakIcon          => Emote.Parse($"<:break:945836553463296050>");
        public static Emote GoldIcon           => Emote.Parse($"<:gold:945836553027067964>");
        public static Emote IllusionIcon       => Emote.Parse($"<:illusion:945836553073213440>");
        public static Emote ManaIcon           => Emote.Parse($"<:mana:945836552838316165>");
        public static Emote BuffIcon           => Emote.Parse($"<:modifier_buff:945836553106755636>");
        public static Emote DebuffIcon         => Emote.Parse($"<:modifier_debuff:945836553052229632>");
        public static Emote ScepterIcon        => Emote.Parse($"<:scepter:945836553161293845>");
        public static Emote ScepterUpgradeIcon => Emote.Parse($"<:scepter_upgade:945836553060638780>");
        public static Emote ShardIcon          => Emote.Parse($"<:shard:945836553584918528>");
        public static Emote ShardUpgradeIcon   => Emote.Parse($"<:shard_upgrade:945836553589112854>");
        public static Emote SpellImmuneIcon    => Emote.Parse($"<:spell_immunity_block:945836553396178975>");
        public static Emote TalentTreeIcon     => Emote.Parse($"<:talent_tree:1014515342598479942>");
        public static Emote RangedIcon         => Emote.Parse($"<:ranged:946541321961742388>");
        public static Emote MeleeIcon          => Emote.Parse($"<:melee:946541322188238938>");
        public static Emote CooldownIcon       => Emote.Parse($"<:cooldown:945836553756880967>");

        public static Emote GetAttributeEmote(this AttributePrimary attribute)
#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            => attribute switch
            {
                AttributePrimary.DOTA_ATTRIBUTE_STRENGTH  => StrengthIcon,
                AttributePrimary.DOTA_ATTRIBUTE_AGILITY   => AgilityIcon,
                AttributePrimary.DOTA_ATTRIBUTE_INTELLECT => IntelligenceIcon,
            };
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.


        public static Emote GetAttackTypeIcon(this AttackCapabilities attackType)
            => attackType == AttackCapabilities.DOTA_UNIT_CAP_MELEE_ATTACK ? MeleeIcon : RangedIcon;
    }
}