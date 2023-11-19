using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Magus.Data.Models.Embeds;

public record Embed
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string ImageUrl { get; set; }
    public string ThumbnailUrl { get; set; }
    [BsonRepresentation(BsonType.Int32, AllowOverflow = true)]
    public uint? ColorRaw { get; set; }
    public IEnumerable<Field>? Fields { get; set; }
    public Footer? Footer { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}

public record Field
{
    public string Name { get; set; }
    public string Value { get; set; }
    public bool IsInline { get; set; }
}

public record Footer
{
    public string Text { get; set; }
    public string IconUrl { get; set; }
}
