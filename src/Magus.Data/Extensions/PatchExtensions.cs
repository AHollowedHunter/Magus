using Magus.Common.Discord;
using Magus.Common.Dota.Models;
using Magus.Data.Enums;
using Magus.Data.Models.V2;

namespace Magus.Data.Extensions;
public static class PatchExtensions
{
    public static PatchNote CreateGeneralNote(this PatchNotes patchNotes, string locale, SerializableEmbed embed)
        => new(locale,
               patchNotes.PatchName,
               patchNotes.Timestamp,
               PatchNoteType.General,
               "General",
               -1,
               EntityType.None,
               embed);

    public static PatchNote CreateHeroNote(this PatchNotes patchNotes, string locale, string internalName, int entityId, SerializableEmbed embed)
        => new(locale,
               patchNotes.PatchName,
               patchNotes.Timestamp,
               PatchNoteType.Hero,
               internalName,
               entityId,
               EntityType.Hero,
               embed);

    public static PatchNote CreateItemNote(this PatchNotes patchNotes, string locale, string internalName, int entityId, SerializableEmbed embed)
        => new(locale,
               patchNotes.PatchName,
               patchNotes.Timestamp,
               PatchNoteType.Item,
               internalName,
               entityId,
               EntityType.Item,
               embed);
}
