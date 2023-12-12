using Magus.Data.Enums;
using System.Text.Json.Serialization;

namespace Magus.Data.Models.V2;
public sealed class Entity : IEntity
{
    public Entity(string internalName, int entityId, EntityType entityType, IDictionary<string, string> name, string[]? aliases = null, string? realName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(internalName);

        InternalName = internalName;
        EntityId = entityId;
        EntityType = entityType;
        Name = name;
        Aliases = aliases;
        RealName = realName;
    }

    [JsonPropertyName(nameof(InternalName))]
    public string InternalName { get; set; }

    [JsonPropertyName(nameof(EntityId))]
    public int EntityId { get; set; }

    [JsonPropertyName(nameof(EntityType))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EntityType EntityType { get; set; }

    [JsonPropertyName(nameof(Name))]
    public IDictionary<string, string> Name { get; set; }

    [JsonPropertyName(nameof(Aliases))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Aliases { get; set; }

    [JsonPropertyName(nameof(RealName))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RealName { get; set; } // TODO needed? how get?
}
