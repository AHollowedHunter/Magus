namespace Magus.Data.Models
{
    internal interface INamedEntity
    {
        public string InternalName { get; set; }
        public string Name { get; set; }
        public string? RealName { get; set; }
        public IEnumerable<string>? Aliases { get; set; }
    }
}
