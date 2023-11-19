namespace Magus.Data.Models;

internal interface ILocalisedEntity : IEntity
{
    public string Name { get; set; }
    public string? RealName { get; set; }
    public IEnumerable<string>? Aliases { get; set; }
}
