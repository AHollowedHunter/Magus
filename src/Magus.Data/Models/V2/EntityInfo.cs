using Magus.Common.Discord;
using Magus.Data.Enums;
using System.Text.Json.Serialization;

namespace Magus.Data.Models.V2;
public sealed class EntityInfo : IEntity, ILocalised
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [JsonConstructor]
    private EntityInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public EntityInfo(string internalName, int entityId, EntityType type, string locale, SerializableEmbed embed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(internalName);
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);

        InternalName = internalName;
        EntityId = entityId;
        EntityType = type;
        Locale = locale;
        Embed = embed ?? throw new ArgumentNullException(nameof(embed));
    }

    public static string MakeUniqueId(string internalName, string locale)
        => $"{internalName}_{locale}";

    /// <summary>
    /// This property is used for a unique reference within the search index.
    /// </summary>
    /// 
    [JsonPropertyName(nameof(UniqueId))]
    public string UniqueId => MakeUniqueId(InternalName, Locale);

    [JsonPropertyName(nameof(Locale))]
    public string Locale { get; set; }

    [JsonPropertyName(nameof(InternalName))]
    public string InternalName { get; set; }

    [JsonPropertyName(nameof(EntityId))]
    public int EntityId { get; set; }

    [JsonPropertyName(nameof(EntityType))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EntityType EntityType { get; set; }

    [JsonPropertyName(nameof(Embed))]
    public SerializableEmbed Embed { get; set; }
}
