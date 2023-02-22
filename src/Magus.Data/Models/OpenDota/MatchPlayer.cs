using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Magus.Data.Models.OpenDota
{
    public sealed record MatchPlayer
    {
        [JsonPropertyName("match_id")]
        public long MatchId { get; set; }
        [JsonPropertyName("player_slot")]
        public long PlayerSlot { get; set; }
        /// <summary>
        /// Total times the ability was used on a target
        /// </summary>
        [JsonPropertyName("ability_targets")]
        public IDictionary<string, IDictionary<string, int>> AbilityTargets { get; set; }
        [JsonPropertyName("ability_upgrades_arr")]
        public long[] AbilityUpgradesArr { get; set; }
        [JsonPropertyName("ability_uses")]
        public Dictionary<string, long> AbilityUses { get; set; }
        [JsonPropertyName("account_id")]
        public long AccountId { get; set; }
        [JsonPropertyName("actions")]
        public Dictionary<string, long> Actions { get; set; }
        //[JsonPropertyName("additional_units")]
        //public object AdditionalUnits { get; set; }
        [JsonPropertyName("assists")]
        public int Assists { get; set; }
        [JsonPropertyName("backpack_0")]
        public long Backpack0 { get; set; }
        [JsonPropertyName("backpack_1")]
        public long Backpack1 { get; set; }
        [JsonPropertyName("backpack_2")]
        public long Backpack2 { get; set; }
        //[JsonPropertyName("backpack_3")]
        //public long Backpack3 { get; set; }
        [JsonPropertyName("buyback_log")]
        public BuybackLog[] BuybackLog { get; set; }
        [JsonPropertyName("camps_stacked")]
        public long CampsStacked { get; set; }
        //[JsonPropertyName("connection_log")]
        //public object[] ConnectionLog { get; set; }
        [JsonPropertyName("creeps_stacked")]
        public long CreepsStacked { get; set; }
        [JsonPropertyName("damage")]
        public Dictionary<string, long> Damage { get; set; }
        [JsonPropertyName("damage_inflictor")]
        public Dictionary<string, long> DamageInflictor { get; set; }
        [JsonPropertyName("damage_inflictor_received")]
        public Dictionary<string, long> DamageInflictorReceived { get; set; }
        [JsonPropertyName("damage_taken")]
        public Dictionary<string, long> DamageTaken { get; set; }
        /// <summary>
        /// Total damage dealt to each target with an ability
        /// </summary>
        [JsonPropertyName("damage_targets")]
        public IDictionary<string, IDictionary<string, int>> DamageTargets { get; set; }
        [JsonPropertyName("deaths")]
        public int Deaths { get; set; }
        [JsonPropertyName("denies")]
        public int Denies { get; set; }
        [JsonPropertyName("dn_t")]
        public long[] DnT { get; set; }
        [JsonPropertyName("firstblood_claimed")]
        public int FirstbloodClaimed { get; set; } // Convert to bool
        [JsonPropertyName("gold")]
        public long Gold { get; set; }
        [JsonPropertyName("gold_per_min")]
        public long GoldPerMin { get; set; }
        [JsonPropertyName("gold_reasons")]
        public Dictionary<string, long> GoldReasons { get; set; }
        [JsonPropertyName("gold_spent")]
        public long GoldSpent { get; set; }
        [JsonPropertyName("gold_t")]
        public long[] GoldT { get; set; }
        [JsonPropertyName("hero_damage")]
        public long HeroDamage { get; set; }
        [JsonPropertyName("hero_healing")]
        public long HeroHealing { get; set; }
        [JsonPropertyName("hero_hits")]
        public Dictionary<string, long> HeroHits { get; set; }
        [JsonPropertyName("hero_id")]
        public long HeroId { get; set; }
        [JsonPropertyName("item_0")]
        public long Item0 { get; set; }
        [JsonPropertyName("item_1")]
        public long Item1 { get; set; }
        [JsonPropertyName("item_2")]
        public long Item2 { get; set; }
        [JsonPropertyName("item_3")]
        public long Item3 { get; set; }
        [JsonPropertyName("item_4")]
        public long Item4 { get; set; }
        [JsonPropertyName("item_5")]
        public long Item5 { get; set; }
        [JsonPropertyName("item_neutral")]
        public long ItemNeutral { get; set; }
        [JsonPropertyName("item_uses")]
        public IDictionary<string, long> ItemUses { get; set; }
        [JsonPropertyName("kill_streaks")]
        public IDictionary<string, long> KillStreaks { get; set; }
        [JsonPropertyName("killed")]
        public IDictionary<string, long> Killed { get; set; }
        [JsonPropertyName("killed_by")]
        public IDictionary<string, long> KilledBy { get; set; }
        [JsonPropertyName("kills")]
        public int Kills { get; set; }
        [JsonPropertyName("kills_log")]
        public KillsLog[] KillsLog { get; set; }
        [JsonPropertyName("lane_pos")]
        public IDictionary<string, IDictionary<string, long>> LanePos { get; set; }
        [JsonPropertyName("last_hits")]
        public long LastHits { get; set; }
        [JsonPropertyName("leaver_status")]
        public long LeaverStatus { get; set; }
        [JsonPropertyName("level")]
        public long Level { get; set; }
        [JsonPropertyName("lh_t")]
        public long[] LhT { get; set; }
        [JsonPropertyName("life_state")]
        public IDictionary<string, long> LifeState { get; set; }
        [JsonPropertyName("max_hero_hit")]
        public MaxHeroHit MaxHeroHit { get; set; }
        [JsonPropertyName("multi_kills")]
        public IDictionary<string, int> MultiKills { get; set; }
        [JsonPropertyName("net_worth")]
        public long NetWorth { get; set; }
        [JsonPropertyName("obs_placed")]
        public long ObsPlaced { get; set; }
        [JsonPropertyName("party_id")]
        public long PartyId { get; set; }
        [JsonPropertyName("party_size")]
        public long PartySize { get; set; }
        //[JsonPropertyName("performance_others")]
        //public object PerformanceOthers { get; set; }
        [JsonPropertyName("permanent_buffs")]
        public PermanentBuff[] PermanentBuffs { get; set; }
        [JsonPropertyName("pings")]
        public long Pings { get; set; }
        [JsonPropertyName("pred_vict")]
        public bool PredVict { get; set; }
        [JsonPropertyName("purchase")]
        public Dictionary<string, int> Purchase { get; set; }
        [JsonPropertyName("purchase_log")]
        public PurchaseLog[] PurchaseLog { get; set; }
        [JsonPropertyName("randomed")]
        public bool Randomed { get; set; }
        //[JsonPropertyName("repicked")]
        //public object Repicked { get; set; }
        [JsonPropertyName("roshans_killed")]
        public long RoshansKilled { get; set; }
        [JsonPropertyName("rune_pickups")]
        public long RunePickups { get; set; }
        [JsonPropertyName("runes")]
        public Dictionary<string, long> Runes { get; set; }
        [JsonPropertyName("runes_log")]
        public RunesLog[] RunesLog { get; set; }
        [JsonPropertyName("sen_placed")]
        public long SenPlaced { get; set; }
        [JsonPropertyName("stuns")]
        public double Stuns { get; set; }
        [JsonPropertyName("teamfight_participation")]
        public double TeamfightParticipation { get; set; }
        [JsonPropertyName("times")]
        public long[] Times { get; set; }
        [JsonPropertyName("tower_damage")]
        public long TowerDamage { get; set; }
        [JsonPropertyName("towers_killed")]
        public long TowersKilled { get; set; }
        [JsonPropertyName("xp_per_min")]
        public long XpPerMin { get; set; }
        [JsonPropertyName("xp_reasons")]
        public Dictionary<string, long> XpReasons { get; set; }
        [JsonPropertyName("xp_t")]
        public long[] XpT { get; set; }
        [JsonPropertyName("personaname")]
        public string Personaname { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("last_login")]
        public DateTimeOffset? LastLogin { get; set; }
        [JsonPropertyName("radiant_win")]
        public bool RadiantWin { get; set; }
        [JsonPropertyName("start_time")]
        public long StartTime { get; set; }
        [JsonPropertyName("duration")]
        public long Duration { get; set; }
        [JsonPropertyName("cluster")]
        public long Cluster { get; set; }
        [JsonPropertyName("lobby_type")]
        public long LobbyType { get; set; }
        [JsonPropertyName("game_mode")]
        public long GameMode { get; set; }
        [JsonPropertyName("is_contributor")]
        public bool IsContributor { get; set; }
        [JsonPropertyName("patch")]
        public long Patch { get; set; }
        [JsonPropertyName("region")]
        public long Region { get; set; }
        [JsonPropertyName("isRadiant")]
        public bool IsRadiant { get; set; }
        [JsonPropertyName("win")]
        public long Win { get; set; }
        [JsonPropertyName("lose")]
        public long Lose { get; set; }
        [JsonPropertyName("total_gold")]
        public long TotalGold { get; set; }
        [JsonPropertyName("total_xp")]
        public long TotalXp { get; set; }
        [JsonPropertyName("kills_per_min")]
        public double? KillsPerMin { get; set; }
        [JsonPropertyName("kda")]
        public long Kda { get; set; }
        [JsonPropertyName("abandons")]
        public long Abandons { get; set; }
        [JsonPropertyName("neutral_kills")]
        public long NeutralKills { get; set; }
        [JsonPropertyName("tower_kills")]
        public long TowerKills { get; set; }
        [JsonPropertyName("courier_kills")]
        public long CourierKills { get; set; }
        [JsonPropertyName("lane_kills")]
        public long LaneKills { get; set; }
        [JsonPropertyName("hero_kills")]
        public long HeroKills { get; set; }
        [JsonPropertyName("observer_kills")]
        public long ObserverKills { get; set; }
        [JsonPropertyName("sentry_kills")]
        public long SentryKills { get; set; }
        [JsonPropertyName("roshan_kills")]
        public long RoshanKills { get; set; }
        [JsonPropertyName("necronomicon_kills")]
        public long NecronomiconKills { get; set; }
        [JsonPropertyName("ancient_kills")]
        public long AncientKills { get; set; }
        [JsonPropertyName("buyback_count")]
        public long BuybackCount { get; set; }
        [JsonPropertyName("observer_uses")]
        public long ObserverUses { get; set; }
        [JsonPropertyName("sentry_uses")]
        public long SentryUses { get; set; }
        [JsonPropertyName("lane_efficiency")]
        public double LaneEfficiency { get; set; }
        [JsonPropertyName("lane_efficiency_pct")]
        public long LaneEfficiencyPct { get; set; }
        [JsonPropertyName("lane")]
        public long Lane { get; set; }
        [JsonPropertyName("lane_role")]
        public long LaneRole { get; set; }
        [JsonPropertyName("is_roaming")]
        public bool IsRoaming { get; set; }
        [JsonPropertyName("purchase_time")]
        public Dictionary<string, long> PurchaseTime { get; set; }
        [JsonPropertyName("first_purchase_time")]
        public Dictionary<string, long> FirstPurchaseTime { get; set; }
        [JsonPropertyName("item_win")]
        public Dictionary<string, long> ItemWin { get; set; }
        [JsonPropertyName("item_usage")]
        public Dictionary<string, long> ItemUsage { get; set; }
        [JsonPropertyName("purchase_ward_observer")]
        public long? PurchaseWardObserver { get; set; }
        [JsonPropertyName("purchase_tpscroll")]
        public long PurchaseTpscroll { get; set; }
        [JsonPropertyName("actions_per_min")]
        public long ActionsPerMin { get; set; }
        [JsonPropertyName("life_state_dead")]
        public long LifeStateDead { get; set; }
        [JsonPropertyName("rank_tier")]
        public long RankTier { get; set; }
        [JsonPropertyName("is_subscriber")]
        public bool IsSubscriber { get; set; }
        //[JsonPropertyName("cosmetics")]
        //public Cosmetic[] Cosmetics { get; set; }
        [JsonPropertyName("benchmarks")]
        public Benchmarks Benchmarks { get; set; }
        [JsonPropertyName("purchase_ward_sentry")]
        public long? PurchaseWardSentry { get; set; }
        [JsonPropertyName("purchase_gem")]
        public long? PurchaseGem { get; set; }
    }

    public sealed record AbilityTarget
    {
        [JsonExtensionData]
        public IDictionary<string, Target> AbilityTargets;

        public sealed record Target
        {
            [JsonExtensionData]
            public IDictionary<string, int> Targets;
        }
    }

    public sealed record Benchmarks
    {
        [JsonPropertyName("gold_per_min")]
        public Dictionary<string, double> GoldPerMin { get; set; }
        [JsonPropertyName("xp_per_min")]
        public Dictionary<string, double> XpPerMin { get; set; }
        [JsonPropertyName("kills_per_min")]
        public Dictionary<string, double> KillsPerMin { get; set; }
        [JsonPropertyName("last_hits_per_min")]
        public Dictionary<string, double> LastHitsPerMin { get; set; }
        [JsonPropertyName("hero_damage_per_min")]
        public Dictionary<string, double> HeroDamagePerMin { get; set; }
        [JsonPropertyName("hero_healing_per_min")]
        public Dictionary<string, double> HeroHealingPerMin { get; set; }
        [JsonPropertyName("tower_damage")]
        public Dictionary<string, double> TowerDamage { get; set; }
        [JsonPropertyName("stuns_per_min")]
        public Dictionary<string, double> StunsPerMin { get; set; }
        [JsonPropertyName("lhten")]
        public Dictionary<string, double> Lhten { get; set; }
    }

    public sealed record BuybackLog
    {
        [JsonPropertyName("time")]
        public long Time { get; set; }
        [JsonPropertyName("slot")]
        public long Slot { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("player_slot")]
        public long PlayerSlot { get; set; }
    }

    public sealed record Cosmetic
    {
        [JsonPropertyName("item_id")]
        public long ItemId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("prefab")]
        public string Prefab { get; set; }
        [JsonPropertyName("creation_date")]
        public DateTimeOffset? CreationDate { get; set; }
        [JsonPropertyName("image_inventory")]
        public string ImageInventory { get; set; }
        [JsonPropertyName("image_path")]
        public string ImagePath { get; set; }
        [JsonPropertyName("item_description")]
        public string ItemDescription { get; set; }
        [JsonPropertyName("item_name")]
        public string ItemName { get; set; }
        [JsonPropertyName("item_rarity")]
        public string ItemRarity { get; set; }
        [JsonPropertyName("item_type_name")]
        public string ItemTypeName { get; set; }
        [JsonPropertyName("used_by_heroes")]
        public string UsedByHeroes { get; set; }
    }

    public sealed record KillsLog
    {
        [JsonPropertyName("time")]
        public long Time { get; set; }
        [JsonPropertyName("key")]
        public string Key { get; set; }
    }

    public sealed record MaxHeroHit
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("time")]
        public long Time { get; set; }
        [JsonPropertyName("max")]
        public bool Max { get; set; }
        [JsonPropertyName("inflictor")]
        public string Inflictor { get; set; }
        [JsonPropertyName("unit")]
        public string Unit { get; set; }
        [JsonPropertyName("key")]
        public string Key { get; set; }
        [JsonPropertyName("value")]
        public long Value { get; set; }
        [JsonPropertyName("slot")]
        public long Slot { get; set; }
        [JsonPropertyName("player_slot")]
        public long PlayerSlot { get; set; }
    }

    public sealed record PermanentBuff
    {
        [JsonPropertyName("permanent_buff")]
        public long PermanentBuffPermanentBuff { get; set; }
        [JsonPropertyName("stack_count")]
        public long StackCount { get; set; }
        [JsonPropertyName("grant_time")]
        public long GrantTime { get; set; }
    }

    public sealed record PurchaseLog
    {
        [JsonPropertyName("time")]
        public long Time { get; set; }
        [JsonPropertyName("key")]
        public string Key { get; set; }
        [JsonPropertyName("charges")]
        public long? Charges { get; set; }
    }

    public sealed record RunesLog
    {
        [JsonPropertyName("time")]
        public long Time { get; set; }
        [JsonPropertyName("key")]
        public long Key { get; set; }
    }
}
