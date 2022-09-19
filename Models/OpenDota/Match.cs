using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Magus.Data.Models.OpenDota
{
    public partial class Match : ISnowflakeRecord
    {
        [JsonPropertyName("match_id")]
        public ulong Id { get; set; }
        [BsonIgnore]
        public ulong MatchId => Id;
        [JsonPropertyName("barracks_status_dire")]
        public long BarracksStatusDire { get; set; }
        [JsonPropertyName("barracks_status_radiant")]
        public long BarracksStatusRadiant { get; set; }
        [JsonPropertyName("chat")]
        public Chat[] Chat { get; set; }
        [JsonPropertyName("cluster")]
        public long Cluster { get; set; }
        [JsonPropertyName("cosmetics")]
        public IDictionary<string, int> Cosmetics { get; set; }
        [JsonPropertyName("dire_score")]
        public long DireScore { get; set; }
        [JsonPropertyName("dire_team_id")]
        public long DireTeamId { get; set; }
        [JsonPropertyName("draft_timings")]
        public DraftTiming[] DraftTimings { get; set; }
        [JsonPropertyName("duration")]
        public long Duration { get; set; }
        [JsonPropertyName("engine")]
        public long Engine { get; set; }
        [JsonPropertyName("first_blood_time")]
        public long FirstBloodTime { get; set; }
        [JsonPropertyName("game_mode")]
        public long GameMode { get; set; }
        [JsonPropertyName("human_players")]
        public long HumanPlayers { get; set; }
        [JsonPropertyName("leagueid")]
        public long Leagueid { get; set; }
        [JsonPropertyName("lobby_type")]
        public long LobbyType { get; set; }
        [JsonPropertyName("match_seq_num")]
        public long MatchSeqNum { get; set; }
        [JsonPropertyName("negative_votes")]
        public long NegativeVotes { get; set; }
        [JsonPropertyName("objectives")]
        public Objective[] Objectives { get; set; }
        [JsonPropertyName("picks_bans")]
        public PicksBan[] PicksBans { get; set; }
        [JsonPropertyName("positive_votes")]
        public long PositiveVotes { get; set; }
        [JsonPropertyName("radiant_gold_adv")]
        public int[] RadiantGoldAdv { get; set; }
        [JsonPropertyName("radiant_score")]
        public long RadiantScore { get; set; }
        [JsonPropertyName("radiant_team_id")]
        public long RadiantTeamId { get; set; }
        [JsonPropertyName("radiant_win")]
        public bool RadiantWin { get; set; }
        [JsonPropertyName("radiant_xp_adv")]
        public int[] RadiantXpAdv { get; set; }
        //[JsonPropertyName("skill")]
        //public object Skill { get; set; }
        [JsonPropertyName("start_time")]
        public long StartTime { get; set; }
        [JsonPropertyName("teamfights")]
        public Teamfight[] Teamfights { get; set; }
        [JsonPropertyName("tower_status_dire")]
        public long TowerStatusDire { get; set; }
        [JsonPropertyName("tower_status_radiant")]
        public long TowerStatusRadiant { get; set; }
        [JsonPropertyName("version")]
        public long Version { get; set; }
        [JsonPropertyName("replay_salt")]
        public long ReplaySalt { get; set; }
        [JsonPropertyName("series_id")]
        public long SeriesId { get; set; }
        [JsonPropertyName("series_type")]
        public long SeriesType { get; set; }
        [JsonPropertyName("league")]
        public League League { get; set; }
        [JsonPropertyName("radiant_team")]
        public Team RadiantTeam { get; set; }
        [JsonPropertyName("dire_team")]
        public Team DireTeam { get; set; }
        [JsonPropertyName("players")]
        public Player[] Players { get; set; }
        [JsonPropertyName("patch")]
        public long Patch { get; set; }
        [JsonPropertyName("region")]
        public long Region { get; set; }
        [JsonPropertyName("all_word_counts")]
        public Dictionary<string, long> AllWordCounts { get; set; }
        [JsonPropertyName("my_word_counts")]
        public Dictionary<string, long> MyWordCounts { get; set; }
        [JsonPropertyName("throw")]
        public long Throw { get; set; }
        [JsonPropertyName("loss")]
        public long Loss { get; set; }
        [JsonPropertyName("replay_url")]
        public Uri ReplayUrl { get; set; }
    }

    public record Objective
    {
        [JsonPropertyName("time")]
        public int Time { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("slot")]
        public int Slot { get; set; }

        /// <summary>
        /// Target
        /// </summary>
        //[JsonPropertyName("key")]
        //public string Key { get; set; }
        // ignored as a painintheass

        [JsonPropertyName("player_slot")]
        public int PlayerSlot { get; set; }

        [JsonPropertyName("team")]
        public int? Team { get; set; }

        /// <summary>
        /// Who dunnit?
        /// </summary>
        [JsonPropertyName("unit")]
        public string Unit { get; set; }
    }

    public record PicksBan
    {
        [JsonPropertyName("is_pick")]
        public bool IsPick { get; set; }
        [JsonPropertyName("hero_id")]
        public int HeroId { get; set; }
        [JsonPropertyName("team")]
        public int Team { get; set; }
        [JsonPropertyName("order")]
        public int Order { get; set; }
    }

    public record DraftTiming
    {
        [JsonPropertyName("order")]
        public int Order { get; set; }

        [JsonPropertyName("pick")]
        public bool Pick { get; set; }

        [JsonPropertyName("active_team")]
        public int ActiveTeam { get; set; }

        [JsonPropertyName("hero_id")]
        public int HeroId { get; set; }

        [JsonPropertyName("player_slot")]
        public int? PlayerSlot { get; set; }

        [JsonPropertyName("extra_time")]
        public int ExtraTime { get; set; }

        [JsonPropertyName("total_time_taken")]
        public int TotalTimeTaken { get; set; }
    }

    public record Teamfight
    {
        [JsonPropertyName("start")]
        public int Start { get; set; }
        [JsonPropertyName("end")]
        public int End { get; set; }

        [JsonPropertyName("last_death")]
        public int LastDeath { get; set; }
        [JsonPropertyName("deaths")]
        public int Deaths { get; set; }

        [JsonPropertyName("players")]
        public IList<Participant> Players { get; set; }
    }

    public record Participant
    {
        // Always empty?
        //[JsonPropertyName("ability_targets")]
        //public IDictionary<string, short> AbilityTargets {get; set;}
        // What do?
        //[JsonPropertyName("deaths_pos")]
        //public DeathsPos DeathsPos {get; set;}

        [JsonPropertyName("ability_uses")]
        public IDictionary<string, short> AbilityUses { get; set; }
        [JsonPropertyName("item_uses")]
        public IDictionary<string, short> ItemUses { get; set; }

        [JsonPropertyName("killed")]
        public IDictionary<string, short> Killed { get; set; }
        [JsonPropertyName("deaths")]
        public int Deaths { get; set; }

        [JsonPropertyName("buybacks")]
        public int Buybacks { get; set; }

        [JsonPropertyName("damage")]
        public int Damage { get; set; }
        [JsonPropertyName("healing")]
        public int Healing { get; set; }

        [JsonPropertyName("gold_delta")]
        public int GoldDelta { get; set; }
        [JsonPropertyName("xp_delta")]
        public int XpDelta { get; set; }
        [JsonPropertyName("xp_start")]
        public int XpStart { get; set; }
        [JsonPropertyName("xp_end")]
        public int XpEnd { get; set; }
    }

    public partial class Cosmetics
    {
    }

    public record Chat
    {
        [JsonPropertyName("time")]
        public int Time { get; set; }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChatType Type { get; set; }

        /// <summary>
        /// Message if chat, or id of chatwheel message
        /// </summary>
        [JsonPropertyName("key")]
        public string Key { get; set; }

        /// <summary>
        /// Appears to be for the hero position at top (0=>9 Left Radiant=>Right Dire)
        /// </summary>
        [JsonPropertyName("slot")]
        public int Slot { get; set; }

        [JsonPropertyName("player_slot")]
        public int PlayerSlot { get; set; }

        public enum ChatType
        {
            chat      = 0x1,
            chatwheel = 0x2,
        }
    }
}
