using Discord;
using Magus.Common.Dota.Enums;

namespace Magus.Common.Emotes;

public static class MagusEmotes
{
    // public static Emote Icon { get; } = Emote.Parse($"<::00000000>");
    public static Emote Spacer { get; } = Emote.Parse("<:spacer:973678534637813760>");

    public static Emote Live { get; } = Emote.Parse($"<a:live:1122107083769258015>"); // in shared emoji server

    public static Emote AnnouncementChannel { get; } = Emote.Parse("<:announcement:1080865323605037108>");
    public static Emote TextChannel { get; } = Emote.Parse("<:text:1080865313110904952>");

    public static Emote MagusIcon { get; } = Emote.Parse("<:magusbot:995727154191020155>");
    public static Emote StratzIcon { get; } = Emote.Parse("<:STRATZ:1113573151549423657>");

    public static Emote MVPIcon { get; } = Emote.Parse("<:mvp:1117570846182092821>");

    public static Emote DotaLogo { get; } = Emote.Parse("<:dota:1080576453449617468>");
    public static Emote DotaLogoAlt { get; } = Emote.Parse("<:dota_alt:1080576919537471661>");

    public static Emote StrengthIcon { get; } = Emote.Parse("<:strength:945826749118291998>");
    public static Emote AgilityIcon { get; } = Emote.Parse("<:agility:945826748539478027>");
    public static Emote IntelligenceIcon { get; } = Emote.Parse("<:intelligence:945826748845678592>");
    public static Emote UniversalIcon { get; } = Emote.Parse("<:universal:1098920779674030142>");

    public static Emote ArmourIcon { get; } = Emote.Parse("<:armor:945826748921167873>");
    public static Emote AttackRangeIcon { get; } = Emote.Parse("<:attack_range:945826749084762112>");
    public static Emote AttackTimeIcon { get; } = Emote.Parse("<:attack_time:945826748996661358>");
    public static Emote DamageIcon { get; } = Emote.Parse("<:damage:945826748992479322>");
    public static Emote MagicResistIcon { get; } = Emote.Parse("<:magic_resist:945826749130883082>");
    public static Emote MoveSpeedIcon { get; } = Emote.Parse("<:movement_speed:945826749571293244>");
    public static Emote ProjectileSpeedIcon { get; } = Emote.Parse("<:projectile_speed:945826748992466954>");
    public static Emote TurnRateIcon { get; } = Emote.Parse("<:turn_rate:945826749009240064>");
    public static Emote VisionIcon { get; } = Emote.Parse("<:vision:945826748895985664>");

    public static Emote BreakIcon { get; } = Emote.Parse("<:break:945836553463296050>");
    public static Emote GoldIcon { get; } = Emote.Parse("<:gold:945836553027067964>");
    public static Emote IllusionIcon { get; } = Emote.Parse("<:illusion:945836553073213440>");
    public static Emote ManaIcon { get; } = Emote.Parse("<:mana:1098946889547796500>");
    public static Emote HpIcon { get; } = Emote.Parse("<:hp:1098946942802862160>");
    public static Emote BuffIcon { get; } = Emote.Parse("<:modifier_buff:945836553106755636>");
    public static Emote DebuffIcon { get; } = Emote.Parse("<:modifier_debuff:945836553052229632>");
    public static Emote ScepterIcon { get; } = Emote.Parse("<:scepter:945836553161293845>");
    public static Emote ScepterUpgradeIcon { get; } = Emote.Parse("<:scepter_upgade:945836553060638780>");
    public static Emote ShardIcon { get; } = Emote.Parse("<:shard:945836553584918528>");
    public static Emote ShardUpgradeIcon { get; } = Emote.Parse("<:shard_upgrade:945836553589112854>");
    public static Emote SpellImmuneIcon { get; } = Emote.Parse("<:spell_immunity_block:945836553396178975>");
    public static Emote TalentTreeIcon { get; } = Emote.Parse("<:talent_tree:1014515342598479942>");
    public static Emote RangedIcon { get; } = Emote.Parse("<:ranged:946541321961742388>");
    public static Emote MeleeIcon { get; } = Emote.Parse("<:melee:946541322188238938>");
    public static Emote CooldownIcon { get; } = Emote.Parse("<:cooldown:1099033446359253074>");

    // Filters
    public static Emote FilterCarry { get; } = Emote.Parse("<:f_carry:1099073806418583653>");
    public static Emote FilterSupport { get; } = Emote.Parse("<:f_supp:1099073805340659752>");
    public static Emote FilterComplexity { get; } = Emote.Parse("<:f_complexity:1099073807689457664>");
    public static Emote FilterDisabler { get; } = Emote.Parse("<:f_disabler:1099073809782415440>");
    public static Emote FilterDurable { get; } = Emote.Parse("<:f_durable:1099073810591912018>");
    public static Emote FilterEscape { get; } = Emote.Parse("<:f_escape:1099073856662159410>");
    public static Emote FilterInitiator { get; } = Emote.Parse("<:f_initiator:1099073874966102077>");
    public static Emote FilterNuker { get; } = Emote.Parse("<:f_nuker:1099073800974372928>");
    public static Emote FilterPusher { get; } = Emote.Parse("<:f_pusher:1099073802337525850>");
    public static Emote FilterMelee { get; } = Emote.Parse("<:f_melee:1099073799401508874>");
    public static Emote FilterRanged { get; } = Emote.Parse("<:f_ranged:1099073803612606594>");

    public static Emote GetAttributeEmote(this AttributePrimary attribute)
        => attribute switch
        {
            AttributePrimary.DOTA_ATTRIBUTE_STRENGTH => StrengthIcon,
            AttributePrimary.DOTA_ATTRIBUTE_AGILITY => AgilityIcon,
            AttributePrimary.DOTA_ATTRIBUTE_INTELLECT => IntelligenceIcon,
            AttributePrimary.DOTA_ATTRIBUTE_ALL => UniversalIcon,
            _ => throw new NotImplementedException(),
        };


    public static Emote GetAttackTypeIcon(this AttackCapabilities attackType)
        => attackType == AttackCapabilities.DOTA_UNIT_CAP_MELEE_ATTACK ? MeleeIcon : RangedIcon;

    public static Emote GetChannelTypeIcon(this IChannel channel)
        => channel.GetChannelType() switch
        {
            ChannelType.Text => TextChannel,
            ChannelType.News => AnnouncementChannel,
            _ => Spacer
        };

    public static Emote GetRoleEmote(this Role role)
        => role switch
        {
            Role.Carry => FilterCarry,
            Role.Disabler => FilterDisabler,
            Role.Durable => FilterDurable,
            Role.Initiator => FilterInitiator,
            Role.Nuker => FilterNuker,
            Role.Pusher => FilterPusher,
            Role.Support => FilterSupport,
            _ => Spacer
        };
}
