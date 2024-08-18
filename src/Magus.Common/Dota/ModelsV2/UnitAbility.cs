namespace Magus.Common.Dota.ModelsV2;

public class UnitAbility : Ability
{
    public bool IsGrantedByScepter { get; set; }

    public bool HasScepterUpgrade { get; set; }

    public bool IsGrantedByShard { get; set; }

    public bool HasShardUpgrade { get; set; }
}
