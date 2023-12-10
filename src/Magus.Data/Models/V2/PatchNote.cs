using Magus.Common.Discord;
using Magus.Data.Enums;
using System.Text.Json.Serialization;

namespace Magus.Data.Models.V2;
public sealed class PatchNote : IEntity, ILocalised, IPatch
{
    public PatchNote(string locale, string patchNumber, ulong timestamp, PatchNoteType patchNoteType, string internalName, int entityId, EntityType entityType, SerializableEmbed embed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentException.ThrowIfNullOrWhiteSpace(patchNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(internalName);

        Locale = locale;
        PatchNumber = patchNumber;
        Timestamp = timestamp;
        PatchNoteType = patchNoteType;
        InternalName = internalName;
        EntityId = entityId;
        EntityType = entityType;
        Embed = embed ?? throw new ArgumentNullException(nameof(embed));
    }

    /// <summary>
    /// This property is used for a unique reference within the search index.
    /// </summary>
    public string UniqueId => $"{PatchNumber}_{InternalName}_{Locale}";

    [JsonPropertyName(nameof(Locale))]
    public string Locale { get; set; }

    [JsonPropertyName(nameof(PatchNumber))]
    public string PatchNumber { get; set; }

    [JsonPropertyName(nameof(Timestamp))]
    public ulong Timestamp { get; set; }

    [JsonPropertyName(nameof(PatchNoteType))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PatchNoteType PatchNoteType { get; set; }

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
