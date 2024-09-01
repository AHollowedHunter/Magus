namespace Magus.Common.Dota.ModelsV2;

public sealed class PatchNoteManifest
{
    public required string  PatchNumber { get; init; }
    public          long    Timestamp   { get; init; }
    public          string? Website     { get; init; }

    public required Note[]       GenericNotes      { get; init; }
    public required HeroNote[]   HeroesNotes       { get; init; }
    public required EntityNote[] ItemNotes         { get; init; }
    public required EntityNote[] NeutralItemNotes  { get; init; }
    public required EntityNote[] NeutralCreepNotes { get; init; }
}

public record Note(int Indent, string? NoteKey, string? InfoKey);

public record EntityNote(string InternalName, string? TitleKey, Note[] Notes);

public record HeroNote(
    string InternalName,
    Note[] General,
    EntityNote[] Abilities,
    EntityNote[] Facets,
    EntityNote? Innate,
    Note[] Talents);
