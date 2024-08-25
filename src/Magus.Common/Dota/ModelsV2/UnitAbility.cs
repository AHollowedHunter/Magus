namespace Magus.Common.Dota.ModelsV2;

public class UnitAbility : Ability
{
    public byte MaxLevel { get; set; }

    public bool IsBreakable { get; set; }

    public bool IsGrantedByScepter { get; set; }

    public bool HasScepterUpgrade { get; set; }

    public bool IsGrantedByShard { get; set; }

    public bool HasShardUpgrade { get; set; }
}
