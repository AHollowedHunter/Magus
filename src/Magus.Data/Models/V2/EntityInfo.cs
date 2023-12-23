using Magus.Common.Discord;
using Magus.Data.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Magus.Data.Models.V2;
public sealed class EntityInfo : IEntity, ILocalised
{
    /// <summary>
    /// Private constructor just for Json, due to the UniqueId property
    /// </summary>
    [JsonConstructor]
    private EntityInfo() { }

    [SetsRequiredMembers]
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
    [JsonPropertyName(nameof(UniqueId))]
    public string UniqueId => MakeUniqueId(InternalName, Locale);

    [JsonPropertyName(nameof(Locale))]
    public required string Locale { get; set; }

    [JsonPropertyName(nameof(InternalName))]
    public required string InternalName { get; set; }

    [JsonPropertyName(nameof(EntityId))]
    public int EntityId { get; set; }

    [JsonPropertyName(nameof(EntityType))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EntityType EntityType { get; set; }

    [JsonPropertyName(nameof(Embed))]
    public required SerializableEmbed Embed { get; set; }
}
