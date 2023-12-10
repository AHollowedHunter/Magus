﻿using Magus.Common.Discord;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Magus.Data.Models.Embeds;

public abstract record BasePatchNoteEmbed : ISnowflakeId, ILocaleRecord
{
    [BsonRepresentation(BsonType.Int64, AllowOverflow = true)]
    public ulong Id { get; set; }
    public string Locale { get; set; }
    public string PatchNumber { get; init; }
    public ulong Timestamp { get; set; }
    public SerializableEmbed Embed { get; init; }
}
