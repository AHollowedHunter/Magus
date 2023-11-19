namespace Magus.Data.Models.Embeds;

public abstract record EntityLoreEmbed : ISnowflakeRecord, ILocaleRecord, ILocalisedEntity
{
    public ulong Id { get; set; }
    public int EntityId { get; set; }
    public string Locale { get; set; }
    public string InternalName { get; set; }
    public string Name { get; set; }
    public string? RealName { get; set; }
    public IEnumerable<string>? Aliases { get; set; }
    public Embed Embed { get; set; }
}
