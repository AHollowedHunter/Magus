using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Magus.Data.Models.Embeds;

public abstract record BasePatchNoteEmbed : ISnowflakeRecord, ILocaleRecord
{
    [BsonRepresentation(BsonType.Int64, AllowOverflow = true)]
    public ulong Id { get; set; }
    public string Locale { get; set; }
    public string PatchNumber { get; init; }
    public ulong Timestamp { get; set; }
    public Embed Embed { get; init; }
}
