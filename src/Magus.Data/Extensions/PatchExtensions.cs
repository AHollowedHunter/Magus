using Magus.Common.Discord;
using Magus.Data.Enums;
using Magus.Data.Models.V2;

namespace Magus.Data.Extensions;
public static class PatchExtensions
{
    public static PatchNote CreateGeneralNote(this Patch patch, string locale, SerializableEmbed embed)
        => new(locale,
               patch.PatchNumber,
               patch.Timestamp,
               PatchNoteType.General,
               "General",
               -1,
               EntityType.None,
               embed);

    public static PatchNote CreateHeroNote(this Patch patch, string locale, string internalName, int entityId, SerializableEmbed embed)
        => new(locale,
               patch.PatchNumber,
               patch.Timestamp,
               PatchNoteType.Hero,
               internalName,
               entityId,
               EntityType.Hero,
               embed);

    public static PatchNote CreateItemNote(this Patch patch, string locale, string internalName, int entityId, SerializableEmbed embed)
        => new(locale,
               patch.PatchNumber,
               patch.Timestamp,
               PatchNoteType.Item,
               internalName,
               entityId,
               EntityType.Item,
               embed);
}
