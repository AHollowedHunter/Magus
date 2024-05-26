﻿using Discord;
using Magus.Common.Attributes;
using System.Reflection;

namespace Magus.Common.Emotes;

public static class HeroEmotes
{
    private static readonly Dictionary<int, Emote> _idEmotes;
    private static readonly Dictionary<string, Emote> _nameEmotes;

    static HeroEmotes()
    {
        var props = typeof(HeroEmotes)
            .GetProperties()
            .Where(x => x.CustomAttributes.Any(x => x.AttributeType == typeof(EntityEmoteAttribute)));
        _idEmotes = props.ToDictionary(x => x.GetCustomAttribute<EntityEmoteAttribute>()!.EntityId, x => (Emote)x.GetValue(null, null)!);
        _nameEmotes = props.ToDictionary(x => x.GetCustomAttribute<EntityEmoteAttribute>()!.InternalName, x => (Emote)x.GetValue(null, null)!);
    }

    public static Emote Get(int heroId)
        => _idEmotes[heroId];

    public static Emote Get(string internalName)
        => _nameEmotes[internalName];

    // Ignore Spelling
    // in Magus Emotes: Heroes 1
    [EntityEmote(102, "npc_dota_hero_abaddon")]
    public static Emote Abaddon { get; } = Emote.Parse($"<:abaddon:1114253825642074222>");
    [EntityEmote(108, "npc_dota_hero_abyssal_underlord")]
    public static Emote AbyssalUnderlord { get; } = Emote.Parse($"<:abyssal_underlord:1114253827906998403>");
    [EntityEmote(73, "npc_dota_hero_alchemist")]
    public static Emote Alchemist { get; } = Emote.Parse($"<:alchemist:1114253835259613294>");
    [EntityEmote(68, "npc_dota_hero_ancient_apparition")]
    public static Emote AncientApparition { get; } = Emote.Parse($"<:ancient_apparition:1114253836824084520>");
    [EntityEmote(1, "npc_dota_hero_antimage")]
    public static Emote Antimage { get; } = Emote.Parse($"<:antimage:1114253839885938788>");
    [EntityEmote(113, "npc_dota_hero_arc_warden")]
    public static Emote ArcWarden { get; } = Emote.Parse($"<:arc_warden:1114253841173585951>");
    [EntityEmote(2, "npc_dota_hero_axe")]
    public static Emote Axe { get; } = Emote.Parse($"<:axe:1114253843706953738>");
    [EntityEmote(3, "npc_dota_hero_bane")]
    public static Emote Bane { get; } = Emote.Parse($"<:bane:1114253845284012132>");
    [EntityEmote(65, "npc_dota_hero_batrider")]
    public static Emote Batrider { get; } = Emote.Parse($"<:batrider:1114253846668120239>");
    [EntityEmote(38, "npc_dota_hero_beastmaster")]
    public static Emote Beastmaster { get; } = Emote.Parse($"<:beastmaster:1114253849008545813>");
    [EntityEmote(4, "npc_dota_hero_bloodseeker")]
    public static Emote Bloodseeker { get; } = Emote.Parse($"<:bloodseeker:1114253850438799461>");
    [EntityEmote(62, "npc_dota_hero_bounty_hunter")]
    public static Emote BountyHunter { get; } = Emote.Parse($"<:bounty_hunter:1114253852905058455>");
    [EntityEmote(78, "npc_dota_hero_brewmaster")]
    public static Emote Brewmaster { get; } = Emote.Parse($"<:brewmaster:1114253854104625212>");
    [EntityEmote(99, "npc_dota_hero_bristleback")]
    public static Emote Bristleback { get; } = Emote.Parse($"<:bristleback:1114253856700899429>");
    [EntityEmote(61, "npc_dota_hero_broodmother")]
    public static Emote Broodmother { get; } = Emote.Parse($"<:broodmother:1114268956279197848>");
    [EntityEmote(96, "npc_dota_hero_centaur")]
    public static Emote Centaur { get; } = Emote.Parse($"<:centaur:1114271827729068155>");
    [EntityEmote(81, "npc_dota_hero_chaos_knight")]
    public static Emote ChaosKnight { get; } = Emote.Parse($"<:chaos_knight:1114271837833142292>");
    [EntityEmote(66, "npc_dota_hero_chen")]
    public static Emote Chen { get; } = Emote.Parse($"<:chen:1114271846171422901>");
    [EntityEmote(56, "npc_dota_hero_clinkz")]
    public static Emote Clinkz { get; } = Emote.Parse($"<:clinkz:1114271853557579796>");
    [EntityEmote(5, "npc_dota_hero_crystal_maiden")]
    public static Emote CrystalMaiden { get; } = Emote.Parse($"<:crystal_maiden:1114271862248190094>");
    [EntityEmote(55, "npc_dota_hero_dark_seer")]
    public static Emote DarkSeer { get; } = Emote.Parse($"<:dark_seer:1114271869890207765>");
    [EntityEmote(119, "npc_dota_hero_dark_willow")]
    public static Emote DarkWillow { get; } = Emote.Parse($"<:dark_willow:1114271882540220496>");
    [EntityEmote(135, "npc_dota_hero_dawnbreaker")]
    public static Emote Dawnbreaker { get; } = Emote.Parse($"<:dawnbreaker:1114271890094161940>");
    [EntityEmote(50, "npc_dota_hero_dazzle")]
    public static Emote Dazzle { get; } = Emote.Parse($"<:dazzle:1114271898881249351>");
    [EntityEmote(43, "npc_dota_hero_death_prophet")]
    public static Emote DeathProphet { get; } = Emote.Parse($"<:death_prophet:1114271907399864330>");
    [EntityEmote(87, "npc_dota_hero_disruptor")]
    public static Emote Disruptor { get; } = Emote.Parse($"<:disruptor:1114271916988047491>");
    [EntityEmote(69, "npc_dota_hero_doom_bringer")]
    public static Emote DoomBringer { get; } = Emote.Parse($"<:doom_bringer:1114271927876456580>");
    [EntityEmote(49, "npc_dota_hero_dragon_knight")]
    public static Emote DragonKnight { get; } = Emote.Parse($"<:dragon_knight:1114271935401054322>");
    [EntityEmote(6, "npc_dota_hero_drow_ranger")]
    public static Emote DrowRanger { get; } = Emote.Parse($"<:drow_ranger:1114271944112623656>");
    [EntityEmote(107, "npc_dota_hero_earth_spirit")]
    public static Emote EarthSpirit { get; } = Emote.Parse($"<:earth_spirit:1114271952929046539>");
    [EntityEmote(7, "npc_dota_hero_earthshaker")]
    public static Emote Earthshaker { get; } = Emote.Parse($"<:earthshaker:1114271960948551732>");
    [EntityEmote(103, "npc_dota_hero_elder_titan")]
    public static Emote ElderTitan { get; } = Emote.Parse($"<:elder_titan:1114271969924370542>");
    [EntityEmote(106, "npc_dota_hero_ember_spirit")]
    public static Emote EmberSpirit { get; } = Emote.Parse($"<:ember_spirit:1114271979906793552>");
    [EntityEmote(58, "npc_dota_hero_enchantress")]
    public static Emote Enchantress { get; } = Emote.Parse($"<:enchantress:1114271989327204422>");
    [EntityEmote(33, "npc_dota_hero_enigma")]
    public static Emote Enigma { get; } = Emote.Parse($"<:enigma:1114271998529511465>");
    [EntityEmote(41, "npc_dota_hero_faceless_void")]
    public static Emote FacelessVoid { get; } = Emote.Parse($"<:faceless_void:1114272009954807818>");
    [EntityEmote(53, "npc_dota_hero_furion")]
    public static Emote Furion { get; } = Emote.Parse($"<:furion:1114272019056447599>");
    [EntityEmote(121, "npc_dota_hero_grimstroke")]
    public static Emote Grimstroke { get; } = Emote.Parse($"<:grimstroke:1114272027998703806>");
    [EntityEmote(72, "npc_dota_hero_gyrocopter")]
    public static Emote Gyrocopter { get; } = Emote.Parse($"<:gyrocopter:1114272037666553996>");
    [EntityEmote(123, "npc_dota_hero_hoodwink")]
    public static Emote Hoodwink { get; } = Emote.Parse($"<:hoodwink:1114272055941156926>");
    [EntityEmote(59, "npc_dota_hero_huskar")]
    public static Emote Huskar { get; } = Emote.Parse($"<:huskar:1114272065801949305>");
    [EntityEmote(74, "npc_dota_hero_invoker")]
    public static Emote Invoker { get; } = Emote.Parse($"<:invoker:1114272077881561268>");
    [EntityEmote(64, "npc_dota_hero_jakiro")]
    public static Emote Jakiro { get; } = Emote.Parse($"<:jakiro:1114272090741284916>");
    [EntityEmote(8, "npc_dota_hero_juggernaut")]
    public static Emote Juggernaut { get; } = Emote.Parse($"<:juggernaut:1114272100354637894>");
    [EntityEmote(90, "npc_dota_hero_keeper_of_the_light")]
    public static Emote KeeperOfTheLight { get; } = Emote.Parse($"<:keeper_of_the_light:1114272110005727242>");
    [EntityEmote(23, "npc_dota_hero_kunkka")]
    public static Emote Kunkka { get; } = Emote.Parse($"<:kunkka:1114272122781569074>");
    [EntityEmote(104, "npc_dota_hero_legion_commander")]
    public static Emote LegionCommander { get; } = Emote.Parse($"<:legion_commander:1114272135129604126>");
    [EntityEmote(52, "npc_dota_hero_leshrac")]
    public static Emote Leshrac { get; } = Emote.Parse($"<:leshrac:1114272145493737566>");
    [EntityEmote(31, "npc_dota_hero_lich")]
    public static Emote Lich { get; } = Emote.Parse($"<:lich:1114272159867617341>");
    [EntityEmote(54, "npc_dota_hero_life_stealer")]
    public static Emote LifeStealer { get; } = Emote.Parse($"<:life_stealer:1114272169262842017>");

    // in Magus Emotes: Heroes 2
    [EntityEmote(25, "npc_dota_hero_lina")]
    public static Emote Lina { get; } = Emote.Parse($"<:lina:1114254953209397339>");
    [EntityEmote(26, "npc_dota_hero_lion")]
    public static Emote Lion { get; } = Emote.Parse($"<:lion:1114254955717591151>");
    [EntityEmote(80, "npc_dota_hero_lone_druid")]
    public static Emote LoneDruid { get; } = Emote.Parse($"<:lone_druid:1114254957063983204>");
    [EntityEmote(48, "npc_dota_hero_luna")]
    public static Emote Luna { get; } = Emote.Parse($"<:luna:1114254958188056766>");
    [EntityEmote(77, "npc_dota_hero_lycan")]
    public static Emote Lycan { get; } = Emote.Parse($"<:lycan:1114254959534424115>");
    [EntityEmote(97, "npc_dota_hero_magnataur")]
    public static Emote Magnataur { get; } = Emote.Parse($"<:magnataur:1114254990027005973>");
    [EntityEmote(136, "npc_dota_hero_marci")]
    public static Emote Marci { get; } = Emote.Parse($"<:marci:1114254991629234277>");
    [EntityEmote(129, "npc_dota_hero_mars")]
    public static Emote Mars { get; } = Emote.Parse($"<:mars:1114254994003202078>");
    [EntityEmote(94, "npc_dota_hero_medusa")]
    public static Emote Medusa { get; } = Emote.Parse($"<:medusa:1114255059572752435>");
    [EntityEmote(82, "npc_dota_hero_meepo")]
    public static Emote Meepo { get; } = Emote.Parse($"<:meepo:1114255072432509100>");
    [EntityEmote(9, "npc_dota_hero_mirana")]
    public static Emote Mirana { get; } = Emote.Parse($"<:mirana:1114255088026918982>");
    [EntityEmote(114, "npc_dota_hero_monkey_king")]
    public static Emote MonkeyKing { get; } = Emote.Parse($"<:monkey_king:1114255098995027989>");
    [EntityEmote(10, "npc_dota_hero_morphling")]
    public static Emote Morphling { get; } = Emote.Parse($"<:morphling:1114255113129820190>");
    [EntityEmote(138, "npc_dota_hero_muerta")]
    public static Emote Muerta { get; } = Emote.Parse($"<:muerta:1114255124467036221>");
    [EntityEmote(89, "npc_dota_hero_naga_siren")]
    public static Emote NagaSiren { get; } = Emote.Parse($"<:naga_siren:1114255134352998452>");
    [EntityEmote(36, "npc_dota_hero_necrolyte")]
    public static Emote Necrolyte { get; } = Emote.Parse($"<:necrolyte:1114255143857303614>");
    [EntityEmote(11, "npc_dota_hero_nevermore")]
    public static Emote Nevermore { get; } = Emote.Parse($"<:nevermore:1114255154955419679>");
    [EntityEmote(60, "npc_dota_hero_night_stalker")]
    public static Emote NightStalker { get; } = Emote.Parse($"<:night_stalker:1114255164690419742>");
    [EntityEmote(88, "npc_dota_hero_nyx_assassin")]
    public static Emote NyxAssassin { get; } = Emote.Parse($"<:nyx_assassin:1114255177151697018>");
    [EntityEmote(76, "npc_dota_hero_obsidian_destroyer")]
    public static Emote ObsidianDestroyer { get; } = Emote.Parse($"<:obsidian_destroyer:1114255186228158534>");
    [EntityEmote(84, "npc_dota_hero_ogre_magi")]
    public static Emote OgreMagi { get; } = Emote.Parse($"<:ogre_magi:1114255200505577633>");
    [EntityEmote(57, "npc_dota_hero_omniknight")]
    public static Emote Omniknight { get; } = Emote.Parse($"<:omniknight:1114255207740743791>");
    [EntityEmote(111, "npc_dota_hero_oracle")]
    public static Emote Oracle { get; } = Emote.Parse($"<:oracle:1114255220160082010>");
    [EntityEmote(120, "npc_dota_hero_pangolier")]
    public static Emote Pangolier { get; } = Emote.Parse($"<:pangolier:1114255228796141628>");
    [EntityEmote(44, "npc_dota_hero_phantom_assassin")]
    public static Emote PhantomAssassin { get; } = Emote.Parse($"<:phantom_assassin:1114255236975050873>");
    [EntityEmote(12, "npc_dota_hero_phantom_lancer")]
    public static Emote PhantomLancer { get; } = Emote.Parse($"<:phantom_lancer:1114255245640482976>");
    [EntityEmote(110, "npc_dota_hero_phoenix")]
    public static Emote Phoenix { get; } = Emote.Parse($"<:phoenix:1114255255337717903>");
    [EntityEmote(137, "npc_dota_hero_primal_beast")]
    public static Emote PrimalBeast { get; } = Emote.Parse($"<:primal_beast:1114255266830106694>");
    [EntityEmote(13, "npc_dota_hero_puck")]
    public static Emote Puck { get; } = Emote.Parse($"<:puck:1114255276984508446>");
    [EntityEmote(14, "npc_dota_hero_pudge")]
    public static Emote Pudge { get; } = Emote.Parse($"<:pudge:1114255290926387220>");
    [EntityEmote(45, "npc_dota_hero_pugna")]
    public static Emote Pugna { get; } = Emote.Parse($"<:pugna:1114255302146146305>");
    [EntityEmote(39, "npc_dota_hero_queenofpain")]
    public static Emote Queenofpain { get; } = Emote.Parse($"<:queenofpain:1114255313453981816>");
    [EntityEmote(51, "npc_dota_hero_rattletrap")]
    public static Emote Rattletrap { get; } = Emote.Parse($"<:rattletrap:1114255323562246144>");
    [EntityEmote(15, "npc_dota_hero_razor")]
    public static Emote Razor { get; } = Emote.Parse($"<:razor:1114255332949110855>");
    [EntityEmote(32, "npc_dota_hero_riki")]
    public static Emote Riki { get; } = Emote.Parse($"<:riki:1114255342243688510>");
    [EntityEmote(86, "npc_dota_hero_rubick")]
    public static Emote Rubick { get; } = Emote.Parse($"<:rubick:1114255354147123290>");
    [EntityEmote(16, "npc_dota_hero_sand_king")]
    public static Emote SandKing { get; } = Emote.Parse($"<:sand_king:1114255367971545178>");
    [EntityEmote(79, "npc_dota_hero_shadow_demon")]
    public static Emote ShadowDemon { get; } = Emote.Parse($"<:shadow_demon:1114255377756856360>");
    [EntityEmote(27, "npc_dota_hero_shadow_shaman")]
    public static Emote ShadowShaman { get; } = Emote.Parse($"<:shadow_shaman:1114255389555437608>");
    [EntityEmote(98, "npc_dota_hero_shredder")]
    public static Emote Shredder { get; } = Emote.Parse($"<:shredder:1114255397860167912>");
    [EntityEmote(75, "npc_dota_hero_silencer")]
    public static Emote Silencer { get; } = Emote.Parse($"<:silencer:1114255407880347679>");
    [EntityEmote(42, "npc_dota_hero_skeleton_king")]
    public static Emote SkeletonKing { get; } = Emote.Parse($"<:skeleton_king:1114255426112987323>");
    [EntityEmote(101, "npc_dota_hero_skywrath_mage")]
    public static Emote SkywrathMage { get; } = Emote.Parse($"<:skywrath_mage:1114255427979464724>");
    [EntityEmote(28, "npc_dota_hero_slardar")]
    public static Emote Slardar { get; } = Emote.Parse($"<:slardar:1114255463924645900>");
    [EntityEmote(93, "npc_dota_hero_slark")]
    public static Emote Slark { get; } = Emote.Parse($"<:slark:1114255495998476298>");
    [EntityEmote(128, "npc_dota_hero_snapfire")]
    public static Emote Snapfire { get; } = Emote.Parse($"<:snapfire:1114255505204990103>");
    [EntityEmote(35, "npc_dota_hero_sniper")]
    public static Emote Sniper { get; } = Emote.Parse($"<:sniper:1114271476296732814>");
    [EntityEmote(67, "npc_dota_hero_spectre")]
    public static Emote Spectre { get; } = Emote.Parse($"<:spectre:1114271490528002118>");
    [EntityEmote(71, "npc_dota_hero_spirit_breaker")]
    public static Emote SpiritBreaker { get; } = Emote.Parse($"<:spirit_breaker:1114271500770488451>");
    [EntityEmote(17, "npc_dota_hero_storm_spirit")]
    public static Emote StormSpirit { get; } = Emote.Parse($"<:storm_spirit:1114271509255573535>");

    // in Magus Emotes: Heroes 3
    [EntityEmote(18, "npc_dota_hero_sven")]
    public static Emote Sven { get; } = Emote.Parse($"<:sven:1114255756628332544>");
    [EntityEmote(105, "npc_dota_hero_techies")]
    public static Emote Techies { get; } = Emote.Parse($"<:techies:1114255766724038787>");
    [EntityEmote(46, "npc_dota_hero_templar_assassin")]
    public static Emote TemplarAssassin { get; } = Emote.Parse($"<:templar_assassin:1114255778572943441>");
    [EntityEmote(109, "npc_dota_hero_terrorblade")]
    public static Emote Terrorblade { get; } = Emote.Parse($"<:terrorblade:1114255789910130810>");
    [EntityEmote(29, "npc_dota_hero_tidehunter")]
    public static Emote Tidehunter { get; } = Emote.Parse($"<:tidehunter:1114255802170089523>");
    [EntityEmote(34, "npc_dota_hero_tinker")]
    public static Emote Tinker { get; } = Emote.Parse($"<:tinker:1114255813041729647>");
    [EntityEmote(19, "npc_dota_hero_tiny")]
    public static Emote Tiny { get; } = Emote.Parse($"<:tiny:1114255831970623610>");
    [EntityEmote(83, "npc_dota_hero_")]
    public static Emote Treant { get; } = Emote.Parse($"<:treant:1114255843693703168>");
    [EntityEmote(95, "npc_dota_hero_troll_warlord")]
    public static Emote TrollWarlord { get; } = Emote.Parse($"<:troll_warlord:1114255857660735488>");
    [EntityEmote(100, "npc_dota_hero_tusk")]
    public static Emote Tusk { get; } = Emote.Parse($"<:tusk:1114255866716246097>");
    [EntityEmote(85, "npc_dota_hero_undying")]
    public static Emote Undying { get; } = Emote.Parse($"<:undying:1114255875360706722>");
    [EntityEmote(70, "npc_dota_hero_ursa")]
    public static Emote Ursa { get; } = Emote.Parse($"<:ursa:1114255884156145814>");
    [EntityEmote(20, "npc_dota_hero_vengefulspirit")]
    public static Emote Vengefulspirit { get; } = Emote.Parse($"<:vengefulspirit:1114255893593346078>");
    [EntityEmote(40, "npc_dota_hero_venomancer")]
    public static Emote Venomancer { get; } = Emote.Parse($"<:venomancer:1114255901801582613>");
    [EntityEmote(47, "npc_dota_hero_viper")]
    public static Emote Viper { get; } = Emote.Parse($"<:viper:1114255910336999454>");
    [EntityEmote(92, "npc_dota_hero_visage")]
    public static Emote Visage { get; } = Emote.Parse($"<:visage:1114255923976876032>");
    [EntityEmote(126, "npc_dota_hero_void_spirit")]
    public static Emote VoidSpirit { get; } = Emote.Parse($"<:void_spirit:1114255935318282273>");
    [EntityEmote(37, "npc_dota_hero_warlock")]
    public static Emote Warlock { get; } = Emote.Parse($"<:warlock:1114255945078411405>");
    [EntityEmote(63, "npc_dota_hero_weaver")]
    public static Emote Weaver { get; } = Emote.Parse($"<:weaver:1114255955719364779>");
    [EntityEmote(21, "npc_dota_hero_windrunner")]
    public static Emote Windrunner { get; } = Emote.Parse($"<:windrunner:1114255965605351534>");
    [EntityEmote(112, "npc_dota_hero_winter_wyvern")]
    public static Emote WinterWyvern { get; } = Emote.Parse($"<:winter_wyvern:1114255975252238390>");
    [EntityEmote(91, "npc_dota_hero_wisp")]
    public static Emote Wisp { get; } = Emote.Parse($"<:wisp:1114255984538427454>");
    [EntityEmote(30, "npc_dota_hero_witch_doctor")]
    public static Emote WitchDoctor { get; } = Emote.Parse($"<:witch_doctor:1114255994994827345>");
    [EntityEmote(22, "npc_dota_hero_zuus")]
    public static Emote Zuus { get; } = Emote.Parse($"<:zuus:1114256006025846895>");
}
