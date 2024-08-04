using Magus.Common.Discord;
using Magus.Data.Enums;
using System.Text.Json.Serialization;

namespace Magus.Data.Models.Dota;
public sealed record EntityInfo : IEntity, ILocalised
{
    /// <summary>
    /// Constructor for Json, due to the UniqueId property already existing.
    /// </summary>
    [JsonConstructor]
    private EntityInfo(string uniqueId, string internalName, int entityId, EntityType type, string locale, SerializableEmbed embed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uniqueId);
        ArgumentException.ThrowIfNullOrWhiteSpace(internalName);
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);

        UniqueId     = uniqueId;
        InternalName = internalName;
        EntityId     = entityId;
        EntityType   = type;
        Locale       = locale;
        Embed        = embed ?? throw new ArgumentNullException(nameof(embed));
    }

    public EntityInfo(string internalName, int entityId, EntityType type, string locale, SerializableEmbed embed) : this(
        MakeUniqueId(internalName, locale),
        internalName,
        entityId,
        type,
        locale,
        embed)
    {
    }

    public static string MakeUniqueId(string internalName, string locale)
        => $"{internalName}_{locale}";

    /// <summary>
    /// This property is used for a unique reference within the search index.
    /// </summary>
    [JsonPropertyName(nameof(UniqueId))]
    public string UniqueId { get; init; }

    [JsonPropertyName(nameof(Locale))]
    public string Locale { get; init; }

    [JsonPropertyName(nameof(InternalName))]
    public string InternalName { get; init; }

    [JsonPropertyName(nameof(EntityId))]
    public int EntityId { get; init; }

    [JsonPropertyName(nameof(EntityType))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EntityType EntityType { get; init; }

    [JsonPropertyName(nameof(Embed))]
    public SerializableEmbed Embed { get; init; }
}
