using Magus.Common.Discord;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Magus.Data.Models.Embeds;

public abstract record EntityInfoEmbed : ISnowflakeId, ILocaleRecord, ILocalisedEntity
{
    [BsonRepresentation(BsonType.Int64, AllowOverflow = true)]
    public ulong Id { get; set; }
    public int EntityId { get; set; }
    public string Locale { get; set; }
    public string InternalName { get; set; }
    public string Name { get; set; }
    public string? RealName { get; set; }
    public IEnumerable<string>? Aliases { get; set; }
    public SerializableEmbed Embed { get; set; }
}
