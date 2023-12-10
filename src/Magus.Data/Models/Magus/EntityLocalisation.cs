using Magus.Data.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Magus.Data.Models.Magus;

public record EntityLocalisation : ISnowflakeId, IEntity
{
    [BsonRepresentation(BsonType.Int64, AllowOverflow = true)]
    public ulong Id { get; set; }
    public int EntityId { get; set; }
    public string InternalName { get; set; }
    public EntityType Type { get; set; }

    /// <summary>
    /// The default IETF language tag.
    /// </summary>
    public string DefaultTag { get; set; }

    /// <summary>
    /// Key is an IETF language tag, value is the localised name.
    /// </summary>
    public IDictionary<string, string> NameLocalisations { get; set; }

    /// <summary>
    /// Gets the default localised name for this record.
    /// </summary>
    public string DefaultName => NameLocalisations[DefaultTag];

    public string GetLocalisedNameOrDefault(string locale)
    {
        NameLocalisations.TryGetValue(locale, out var name);
        return name ?? DefaultName;
    }
}
