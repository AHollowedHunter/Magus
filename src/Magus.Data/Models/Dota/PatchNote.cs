using Magus.Common.Discord;
using Magus.Data.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Magus.Data.Models.Dota;
public sealed class PatchNote : IEntity, ILocalised, IPatch
{
    /// <summary>
    /// Private constructor just for Json, due to the UniqueId property
    /// </summary>
    [JsonConstructor]
    private PatchNote() { }

    [SetsRequiredMembers]
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

    public static string MakeUniqueId(string patchNumber, string internalName, string locale)
        => $"{patchNumber.Replace('.', '-')}_{internalName}_{locale}";

    /// <summary>
    /// This property is used for a unique reference within the search index.
    /// </summary>
    [JsonPropertyName(nameof(UniqueId))]
    public string UniqueId => MakeUniqueId(PatchNumber, InternalName, Locale);

    [JsonPropertyName(nameof(Locale))]
    public required string Locale { get; set; }

    [JsonPropertyName(nameof(PatchNumber))]
    public required string PatchNumber { get; set; }

    [JsonPropertyName(nameof(Timestamp))]
    public ulong Timestamp { get; set; }

    [JsonPropertyName(nameof(PatchNoteType))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PatchNoteType PatchNoteType { get; set; }

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
