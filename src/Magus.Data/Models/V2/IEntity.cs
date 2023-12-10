using Magus.Data.Enums;

namespace Magus.Data.Models.V2;

internal interface IEntity
{
    public int EntityId { get; }
    public string InternalName { get; }
    public EntityType Type { get; }
}
