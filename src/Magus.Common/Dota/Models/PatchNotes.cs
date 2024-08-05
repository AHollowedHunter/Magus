namespace Magus.Common.Dota.Models;

public record PatchNotes
{
    public string PatchName { get; set; }
    public long Timestamp { get; set; }
    public string Language { get; set; }
    public string? Website { get; set; }
    public IList<Note> GenericNotes { get; set; } = new List<Note>();
    public IList<AbilityNotes> ItemNotes { get; set; } = new List<AbilityNotes>();
    public IList<AbilityNotes> NeutralItemNotes { get; set; } = new List<AbilityNotes>();
    public IList<HeroNotes> HeroesNotes { get; set; } = new List<HeroNotes>();
    public IList<AbilityNotes> NeutralCreepNotes { get; set; } = new List<AbilityNotes>();

    public record Note
    {
        public int Indent { get; set; }
        public string Value { get; set; }
        public string? Info { get; set; }
    }

    public record AbilityNotes
    {
        public string InternalName { get; set; }
        public string? Title { get; set; }
        public IList<Note> Notes { get; set; } = new List<Note>();
    }

    public record HeroNotes
    {
        public string InternalName { get; set; }
        public IList<AbilityNotes> AbilityNotes { get; set; } = new List<AbilityNotes>();
        public IList<Note> GeneralNotes { get; set; } = new List<Note>();
        public IList<Note> TalentNotes { get; set; } = new List<Note>();
    }
}
