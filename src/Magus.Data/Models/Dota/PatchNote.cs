using Magus.Common.Discord;
using Magus.Data.Enums;
using System.Text.Json.Serialization;

namespace Magus.Data.Models.Dota;

public sealed record PatchNote : IEntity, ILocalised, IPatch
{
    /// <summary>
    /// Constructor for Json, due to the UniqueId property already existing.
    /// </summary>
    [JsonConstructor]
    private PatchNote(
        string uniqueId,
        string locale,
        string patchNumber,
        ulong timestamp,
        PatchNoteType patchNoteType,
        string internalName,
        int entityId,
        EntityType entityType,
        SerializableEmbed embed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uniqueId);
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentException.ThrowIfNullOrWhiteSpace(patchNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(internalName);

        UniqueId      = uniqueId;
        Locale        = locale;
        PatchNumber   = patchNumber;
        Timestamp     = timestamp;
        PatchNoteType = patchNoteType;
        InternalName  = internalName;
        EntityId      = entityId;
        EntityType    = entityType;
        Embed         = embed ?? throw new ArgumentNullException(nameof(embed));
    }

    public PatchNote(
        string locale,
        string patchNumber,
        ulong timestamp,
        PatchNoteType patchNoteType,
        string internalName,
        int entityId,
        EntityType entityType,
        SerializableEmbed embed)
        : this(
            MakeUniqueId(patchNumber, internalName, locale),
            locale,
            patchNumber,
            timestamp,
            patchNoteType,
            internalName,
            entityId,
            entityType,
            embed)
    {
    }

    public static string MakeUniqueId(string patchNumber, string internalName, string locale)
        => $"{patchNumber.Replace('.', '-')}_{internalName}_{locale}";

    /// <summary>
    /// This property is used for a unique reference within the search index.
    /// </summary>
    [JsonPropertyName(nameof(UniqueId))]
    public string UniqueId { get; init; }

    [JsonPropertyName(nameof(Locale))]
    public string Locale { get; init; }

    [JsonPropertyName(nameof(PatchNumber))]
    public string PatchNumber { get; init; }

    [JsonPropertyName(nameof(Timestamp))]
    public ulong Timestamp { get; init; }

    [JsonPropertyName(nameof(PatchNoteType))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PatchNoteType PatchNoteType { get; init; }

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
