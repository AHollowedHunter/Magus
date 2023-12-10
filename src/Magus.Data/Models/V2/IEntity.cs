using Magus.Data.Enums;

namespace Magus.Data.Models.V2;

public interface IEntity
{
    public int EntityId { get; }
    public string InternalName { get; }
    public EntityType EntityType { get; }
}
