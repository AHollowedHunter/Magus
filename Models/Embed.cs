﻿namespace Magus.Data.Models
{
    public record Embed
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public uint? ColorRaw { get; set; }
        public IList<Field>? Fields { get; set; }
        public Footer Footer { get; set; }
        public DateTimeOffset? TimeStamp { get; set; }
    }

    public struct Field
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Inline { get; set; }
    }

    public struct Footer
    {
        public string Text { get; set; }
        public string IconUrl { get; set; }
    }
}
