namespace Magus.Common.Dota.Models;

public record Ability : BaseSpell
{
    public IDictionary<string, Talent>? LinkedTalents { get; set; } = new Dictionary<string, Talent>();

    public bool AbilityHasScepter { get; set; }
    public bool AbilityHasShard { get; set; }
    public bool AbilityIsGrantedByScepter { get; set; }
    public bool AbilityIsGrantedByShard { get; set; }

    public string? ScepterDescription { get; set; }
    public string? ShardDescription { get; set; }

    public IList<UpgradeValues> ScepterValues { get; set; }
    public IList<UpgradeValues> ShardValues { get; set; }

    public record UpgradeValues
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public IList<float> Values { get; set; }
    }
}
