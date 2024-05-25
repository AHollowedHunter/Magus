using Magus.Common.Enums;

namespace Magus.Data.Models;

public record Announcement : ISnowflakeId, ILocalised
{
    public ulong Id { get; set; }

    public Topic Topic { get; set; }

    public bool IsPublished { get; set; }

    public string Url { get; set; }

    public string? ImageUrl { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    /// <remarks>
    /// Unix Timestamp
    /// </remarks>
    public long Date { get; set; }

    public string Locale { get; set; }
}
