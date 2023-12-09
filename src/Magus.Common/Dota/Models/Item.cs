using Magus.Common.Dota.Models;

namespace Magus.Common.Dota.Models;

public record Item : BaseSpell
{
    public IEnumerable<string>? ItemAliases { get; set; }

    public short ItemCost { get; set; } // if cost is 0, and item is ItemRecipe, item does NOT exist to buy, even if ItemPurchasable is set... figure this out
    public byte ItemInitialCharges { get; set; }
    public byte ItemStockInitial { get; set; }
    public byte ItemStockMax { get; set; }
    public float ItemStockTime { get; set; }
    public float ItemInitialStockTime { get; set; }

    public bool ItemIsNeutralDrop { get; set; }
    public byte ItemNeutralTier { get; set; }

    public byte MaxUpgradeLevel { get; set; }
    public byte ItemBaseLevel { get; set; }

    public bool ItemRecipe { get; set; }
    public string? ItemResult { get; set; }
    public IEnumerable<string[]>? ItemRequirements { get; set; }

    public bool IsObsolete { get; set; } = false;
    public bool ItemPurchasable { get; set; } = true;

    public IList<Spell>? Spells { get; set; }

    public record Spell
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
