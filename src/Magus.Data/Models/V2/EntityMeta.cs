using Magus.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Magus.Data.Models.V2;
public sealed class EntityMeta
{
    [Key]
    [JsonPropertyName(nameof(InternalName))]
    public string InternalName { get; set; }

    [JsonPropertyName(nameof(EntityId))]
    public int EntityId { get; set; }

    [JsonPropertyName(nameof(Type))]
    public EntityType Type { get; set; }

    [JsonPropertyName(nameof(Name))]
    public IDictionary<string, string> Name { get; set; }

    [JsonPropertyName(nameof(Aliases))]
    public string[]? Aliases { get; set; }

    [JsonPropertyName(nameof(RealName))]
    public string? RealName { get; set; } // TODO needed? how get?

    public EntityMeta(string internalName, int entityId, EntityType type, IDictionary<string, string> name, string[]? aliases = null, string? realName = null)
    {
        InternalName = internalName;
        EntityId = entityId;
        Type = type;
        Name = name;
        Aliases = aliases;
        RealName = realName;
    }
}
