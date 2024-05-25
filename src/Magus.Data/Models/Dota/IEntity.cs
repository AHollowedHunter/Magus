using Magus.Data.Enums;

namespace Magus.Data.Models.Dota;

public interface IEntity
{
    public int EntityId { get; }
    public string InternalName { get; }
    public EntityType EntityType { get; }
}
