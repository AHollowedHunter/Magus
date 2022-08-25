namespace Magus.Data.Models.Dota
{
    public record Ability : BaseSpell
    {
        public bool AbilityHasScepter { get; set; }
        public bool AbilityHasShard { get; set; }
        public bool AbilityIsGrantedByScepter { get; set; }
        public bool AbilityIsGrantedByShard { get; set; }
    }
}
