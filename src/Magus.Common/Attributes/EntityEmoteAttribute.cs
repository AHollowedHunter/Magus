namespace Magus.Common.Attributes;


[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class EntityEmoteAttribute : Attribute
{
    /// <summary>
    /// Mark an emote with the entity id
    /// </summary>
    /// <param name="entityId">The entities ID</param>
    public EntityEmoteAttribute(int entityId)
    {
        EntityId = entityId;
    }

    /// <summary>
    /// The Entities ID
    /// </summary>
    public int EntityId { get; }
}
