using Discord;
using Magus.Common.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Magus.Common.Emotes;

public static class FacetEmotes
{
    private static readonly Dictionary<string, Emote> _emotes;

    static FacetEmotes()
    {
        var props = typeof(FacetEmotes)
            .GetProperties()
            .Where(x => x.CustomAttributes.Any(x => x.AttributeType == typeof(ResourceAttribute)));
        _emotes = props.ToDictionary(x => x.GetCustomAttribute<ResourceAttribute>()!.Name, x => (Emote)x.GetValue(null, null)!);
    }

    /// <summary>
    /// Get Emote via Resource Icon Name
    /// </summary>
    /// <exception cref="KeyNotFoundException"/>
    public static Emote Get(string name)
        => _emotes[name];

    /// <summary>
    /// Get Emote via Resource Icon Name
    /// </summary>
    public static bool TryGet(string name, [MaybeNullWhen(false)] out Emote result)
        => _emotes.TryGetValue(name, out result);

    [Resource("agility")]
    public static Emote Agility { get; } = Emote.Parse("<:facet_agility:1244377489237606470>");
    [Resource("arc_warden")]
    public static Emote ArcWarden { get; } = Emote.Parse("<:facet_arc_warden:1244377496216928389>");
    [Resource("arc_warden_alt")]
    public static Emote ArcWardenAlt { get; } = Emote.Parse("<:facet_arc_warden_alt:1244377498108694588>");
    [Resource("area_of_effect")]
    public static Emote AreaOfEffect { get; } = Emote.Parse("<:facet_area_of_effect:1244377499568308366>");
    [Resource("armor")]
    public static Emote Armor { get; } = Emote.Parse("<:facet_armor:1244377500889383082>");
    [Resource("armor_broken")]
    public static Emote ArmorBroken { get; } = Emote.Parse("<:facet_armor_broken:1244377501980168213>");
    [Resource("barrier")]
    public static Emote Barrier { get; } = Emote.Parse("<:facet_barrier:1244377503414354030>");
    [Resource("broken_chain")]
    public static Emote BrokenChain { get; } = Emote.Parse("<:facet_broken_chain:1244377504941342842>");
    [Resource("brush")]
    public static Emote Brush { get; } = Emote.Parse("<:facet_brush:1244377505608110192>");
    [Resource("chen")]
    public static Emote Chen { get; } = Emote.Parse("<:facet_chen:1244377507004944436>");
    [Resource("chicken")]
    public static Emote Chicken { get; } = Emote.Parse("<:facet_chicken:1244378037957689390>");
    [Resource("chrono_cube")]
    public static Emote ChronoCube { get; } = Emote.Parse("<:facet_chrono_cube:1244377980289941604>");
    [Resource("cooldown")]
    public static Emote Cooldown { get; } = Emote.Parse("<:facet_cooldown:1244377968713793557>");
    [Resource("creep")]
    public static Emote Creep { get; } = Emote.Parse("<:facet_creep:1244377969775087677>");
    [Resource("curve_ball")]
    public static Emote CurveBall { get; } = Emote.Parse("<:facet_curve_ball:1244377970659950613>");
    [Resource("damage")]
    public static Emote Damage { get; } = Emote.Parse("<:facet_damage:1244377972496928798>");
    [Resource("dawnbreaker_hammer")]
    public static Emote DawnbreakerHammer { get; } = Emote.Parse("<:facet_dawnbreaker_hammer:1244377973356892181>");
    [Resource("death_ward")]
    public static Emote DeathWard { get; } = Emote.Parse("<:facet_death_ward:1244377974963441695>");
    [Resource("debuff")]
    public static Emote Debuff { get; } = Emote.Parse("<:facet_debuff:1244377976167075871>");
    [Resource("double_bounce")]
    public static Emote DoubleBounce { get; } = Emote.Parse("<:facet_double_bounce:1244377977215651892>");
    [Resource("dragon_fire")]
    public static Emote DragonFire { get; } = Emote.Parse("<:facet_dragon_fire:1244378544382017678>");
    [Resource("dragon_frost")]
    public static Emote DragonFrost { get; } = Emote.Parse("<:facet_dragon_frost:1244378518838706177>");
    [Resource("dragon_poison")]
    public static Emote DragonPoison { get; } = Emote.Parse("<:facet_dragon_poison:1244378508063543377>");
    [Resource("fence")]
    public static Emote Fence { get; } = Emote.Parse("<:facet_fence:1244378509481082980>");
    [Resource("fist")]
    public static Emote Fist { get; } = Emote.Parse("<:facet_fist:1244378510684852224>");
    [Resource("gold")]
    public static Emote Gold { get; } = Emote.Parse("<:facet_gold:1244378511653994546>");
    [Resource("healing")]
    public static Emote Healing { get; } = Emote.Parse("<:facet_healing:1244378512664825866>");
    [Resource("illusion")]
    public static Emote Illusion { get; } = Emote.Parse("<:facet_illusion:1244378513511940267>");
    [Resource("intelligence")]
    public static Emote Intelligence { get; } = Emote.Parse("<:facet_intelligence:1244378514656989265>");
    [Resource("invoker_active")]
    public static Emote InvokerActive { get; } = Emote.Parse("<:facet_invoker_active:1244378515990773810>");
    [Resource("invoker_passive")]
    public static Emote InvokerPassive { get; } = Emote.Parse("<:facet_invoker_passive:1244378733431881822>");
    [Resource("item")]
    public static Emote Item { get; } = Emote.Parse("<:facet_item:1244378735101083709>");
    [Resource("mana")]
    public static Emote Mana { get; } = Emote.Parse("<:facet_mana:1244378736003121173>");
    [Resource("meat")]
    public static Emote Meat { get; } = Emote.Parse("<:facet_meat:1244378737445703710>");
    [Resource("moon")]
    public static Emote Moon { get; } = Emote.Parse("<:facet_moon:1244378738104209541>");
    [Resource("movement")]
    public static Emote Movement { get; } = Emote.Parse("<:facet_movement:1244378739580600380>");
    [Resource("multi_arrow")]
    public static Emote MultiArrow { get; } = Emote.Parse("<:facet_multi_arrow:1244378731405901824>");
    [Resource("no_vision")]
    public static Emote NoVision { get; } = Emote.Parse("<:facet_no_vision:1244378732207149117>");
    [Resource("nuke")]
    public static Emote Nuke { get; } = Emote.Parse("<:facet_nuke:1244379806993481822>");
    [Resource("ogre")]
    public static Emote Ogre { get; } = Emote.Parse("<:facet_ogre:1244379808356499548>");
    [Resource("overshadow")]
    public static Emote Overshadow { get; } = Emote.Parse("<:facet_overshadow:1244379809052627046>");
    [Resource("pudge_hook")]
    public static Emote PudgeHook { get; } = Emote.Parse("<:facet_pudge_hook:1244379811217018910>");
    [Resource("range")]
    public static Emote Range { get; } = Emote.Parse("<:facet_range:1244379812521574441>");
    [Resource("ricochet")]
    public static Emote Ricochet { get; } = Emote.Parse("<:facet_ricochet:1244379813150724158>");
    [Resource("rng")]
    public static Emote Rng { get; } = Emote.Parse("<:facet_rng:1244379805525475429>");
    [Resource("siege")]
    public static Emote Siege { get; } = Emote.Parse("<:facet_siege:1244379841059487794>");
    [Resource("silencer")]
    public static Emote Silencer { get; } = Emote.Parse("<:facet_silencer:1244379842145681529>");
    [Resource("slow")]
    public static Emote Slow { get; } = Emote.Parse("<:facet_slow:1244379843270017155>");
    [Resource("snake")]
    public static Emote Snake { get; } = Emote.Parse("<:facet_snake:1244379844821647501>");
    [Resource("snot")]
    public static Emote Snot { get; } = Emote.Parse("<:facet_snot:1244379839868178563>");
    [Resource("snowflake")]
    public static Emote Snowflake { get; } = Emote.Parse("<:facet_snowflake:1244380072950108240>");
    [Resource("spectre")]
    public static Emote Spectre { get; } = Emote.Parse("<:facet_spectre:1244380074753523833>");
    [Resource("speed")]
    public static Emote Speed { get; } = Emote.Parse("<:facet_speed:1244380076397559928>");
    [Resource("spinning")]
    public static Emote Spinning { get; } = Emote.Parse("<:facet_spinning:1244380077681152060>");
    [Resource("spirit")]
    public static Emote Spirit { get; } = Emote.Parse("<:facet_spirit:1244380071591022666>");
    [Resource("strength")]
    public static Emote Strength { get; } = Emote.Parse("<:facet_strength:1244380119645028443>");
    [Resource("summons")]
    public static Emote Summons { get; } = Emote.Parse("<:facet_summons:1244380114456674414>");
    [Resource("sun")]
    public static Emote Sun { get; } = Emote.Parse("<:facet_sun:1244380115383881841>");
    [Resource("teleport")]
    public static Emote Teleport { get; } = Emote.Parse("<:facet_teleport:1244380116801294356>");
    [Resource("tree")]
    public static Emote Tree { get; } = Emote.Parse("<:facet_tree:1244380118000861305>");
    [Resource("twin_hearts")]
    public static Emote TwinHearts { get; } = Emote.Parse("<:facet_twin_hearts:1244380284330184835>");
    [Resource("unique")]
    public static Emote Unique { get; } = Emote.Parse("<:facet_unique:1244380285001531464>");
    [Resource("vision")]
    public static Emote Vision { get; } = Emote.Parse("<:facet_vision:1244380286305828924>");
    [Resource("vortex_in")]
    public static Emote VortexIn { get; } = Emote.Parse("<:facet_vortex_in:1244380287606198394>");
    [Resource("vortex_out")]
    public static Emote VortexOut { get; } = Emote.Parse("<:facet_vortex_out:1244380289015218247>");
    [Resource("wolf")]
    public static Emote Wolf { get; } = Emote.Parse("<:facet_wolf:1244380290521235486>");
    [Resource("xp")]
    public static Emote Xp { get; } = Emote.Parse("<:facet_xp:1244380291590787193>");
}
