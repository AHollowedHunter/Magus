using Magus.Common.Discord;
using Magus.Data.Models.Dota;

namespace Magus.Data.Extensions;
public static class EntityExtensions
{
    public static EntityInfo CreateInfo(this Entity entity, string locale, SerializableEmbed embed)
        => new(entity.InternalName,
               entity.EntityId,
               entity.EntityType,
               locale,
               embed);
}
