namespace Magus.Data.Models.Embeds;

public record TalentInfoEmbed : EntityInfoEmbed
{
    public ulong HeroId { get; set; }
}
