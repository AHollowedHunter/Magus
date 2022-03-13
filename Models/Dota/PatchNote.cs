using System.Text.Json.Serialization;

namespace Magus.Data.Models.Dota
{
    public abstract record PatchNote
    {
        [JsonPropertyName("patch_number")]
        public string PatchNumber { get; init; }
        //public PatchNoteType PatchNoteType { get; init; }
    }

    public record GenericPatchNote : PatchNote
    {
        public IList<Note> Notes { get; init; }
    }

    /// <summary>
    /// As of 2022-02-15, dota datafeed returns items with the properties named as "ability", hence the vastly different property and json names
    /// </summary>
    public record ItemPatchNote : PatchNote
    {
        [JsonPropertyName("ability_id")]
        public int ItemId { get; init; }
        [JsonPropertyName("note")]
        public IList<Note> Note { get; init; }
    }

    public record HeroPatchNote : PatchNote
    {
        [JsonPropertyName("hero_id")]
        public int HeroId { get; init; }
        [JsonPropertyName("hero_notes")]
        public IList<Note> HeroNotes { get; init; }
        [JsonPropertyName("abilities")]
        public IList<AbilityNote> Abilities { get; init; }
        [JsonPropertyName("talent_notes")]
        public IList<Note> Talents { get; init; }
    }

    public record AbilityNote
    {
        [JsonPropertyName("ability_id")]
        public int AbilityId { get; init; }
        [JsonPropertyName("ability_notes")]
        public IList<Note> Notes { get; init; }
    }

    public record Note
    {
        [JsonPropertyName("note")]
        public string Content { get; init; }
        [JsonPropertyName("indent_level")]
        public int IndentLevel { get; init; }
    }
}
