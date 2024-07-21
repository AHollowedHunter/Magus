using Magus.Common.Enums;

namespace Magus.Data.Models;

public record Announcement : ISnowflakeId, ILocalised
{
    public ulong Id { get; set; }

    public Topic Topic { get; set; }

    public bool IsPublished { get; set; }

    public required string Url { get; set; }

    public string? ImageUrl { get; set; }

    public required string Title { get; set; }

    public required string Content { get; set; }

    /// <remarks>
    /// Unix Timestamp
    /// </remarks>
    public long Date { get; set; }

    public required string Locale { get; set; }
}
