using Magus.Data.Enums;
using System.Text.Json.Serialization;

namespace Magus.Data.Models.Dota;

public sealed record Entity : IEntity
{
    public Entity(
        string internalName,
        int entityId,
        EntityType entityType,
        IDictionary<string, string> name,
        string[]? aliases = null,
        string? realName = null,
        string[]? linkedEntities = null,
        string[]? entityFilters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(internalName);

        InternalName   = internalName;
        EntityId       = entityId;
        EntityType     = entityType;
        Name           = name;
        Aliases        = aliases;
        RealName       = realName;
        LinkedEntities = linkedEntities;
        EntityFilters  = entityFilters;
    }

    [JsonPropertyName(nameof(InternalName))]
    public string InternalName { get; init; }

    [JsonPropertyName(nameof(EntityId))]
    public int EntityId { get; init; }

    [JsonPropertyName(nameof(EntityType))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EntityType EntityType { get; init; }

    [JsonPropertyName(nameof(Name))]
    public IDictionary<string, string> Name { get; init; }

    [JsonPropertyName(nameof(Aliases))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Aliases { get; init; }

    [JsonPropertyName(nameof(RealName))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RealName { get; init; } // TODO needed? how get?

    /// <summary>
    /// This should be an array of other <see cref="InternalName"/>'s
    /// </summary>
    [JsonPropertyName(nameof(LinkedEntities))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? LinkedEntities { get; init; }

    /// <summary>
    /// Array of additional values to filter against.
    /// </summary>
    /// <remarks>
    /// Recommend to use <see cref="Constants.EntityFilter"/> for values.
    /// </remarks>
    [JsonPropertyName(nameof(EntityFilters))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? EntityFilters { get; init; }
}
