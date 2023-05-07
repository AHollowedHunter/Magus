namespace Magus.Data.Models.Embeds
{
    public record AbilityInfoEmbed : EntityInfoEmbed
    {
        public int HeroId { get; set; }
        public bool Scepter { get; set; }
        public bool Shard { get; set; }
    }
}
