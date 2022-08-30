namespace Magus.Data.Models.Embeds
{
    public record TalentInfo : EntityInfoEmbed
    {
        public ulong HeroId { get; set; }
    }
}
