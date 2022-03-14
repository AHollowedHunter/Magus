namespace Magus.Data.Models.DotaEmbeds
{
    public record HeroPatchNote : BasePatchNote
    {
        public int HeroId { get; init; }
    }
}
