namespace Magus.Common.Dota.ModelsV2;

public sealed class PatchNoteManifest
{
    public required string  PatchNumber { get; init; }
    public          long    Timestamp   { get; init; }
    public          string? Website     { get; init; }

    public required NoteGroup[] GenericNotes      { get; init; }
    public required HeroNote[]  HeroesNotes       { get; init; }
    public required NoteGroup[] ItemNotes         { get; init; }
    public required NoteGroup[] NeutralItemNotes  { get; init; }
    public required NoteGroup[] NeutralCreepNotes { get; init; }
}

public record Note(int Indent, string? NoteKey, string? InfoKey, bool IsScepter, bool IsShard);

public record NoteGroup(string InternalName, string? TitleKey, Note[] Notes, bool IsGeneral);

public record HeroNote(
    string InternalName,
    Note[] General,
    NoteGroup[] Abilities,
    NoteGroup[] Facets,
    NoteGroup? Innate,
    Note[] Talents);
