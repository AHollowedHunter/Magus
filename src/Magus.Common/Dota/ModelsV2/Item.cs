using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Magus.Common.Dota.ModelsV2;

public class Item : Ability
{
    public required string[] ItemAliases { get; set; }

    public int ItemCost { get; set; } // if cost is 0, and item is ItemRecipe, item does NOT exist to buy, even if ItemPurchasable is set... figure this out

    public int ItemInitialCharges { get; set; }

    public bool ItemRequiresCharges { get; set; }

    public int? ItemStockInitial { get; set; }

    public int? ItemStockMax { get; set; }

    public float? ItemStockTime { get; set; }

    public float? ItemInitialStockTime { get; set; }

    public bool ItemIsNeutralDrop { get; set; }

    public byte? MaxUpgradeLevel { get; set; }

    public byte? ItemBaseLevel { get; set; }

    public bool ItemDroppable { get; set; }

    public bool ItemPurchasable { get; set; }

    public bool ItemSellable { get; set; }

    public bool IsObsolete { get; set; }

    [MemberNotNullWhen(true, nameof(ItemResult), nameof(ItemRequirements))]
    public bool ItemRecipe { get; set; }

    public string? ItemResult { get; set; }

    public ItemRequirement[][]? ItemRequirements { get; set; }

    [DebuggerDisplay("{ItemName} {ReplaceItemSlot}")]
    public readonly struct ItemRequirement(string itemName, bool replaceItemSlot = false)
    {
        public string ItemName        { get; } = itemName;
        public bool   ReplaceItemSlot { get; } = replaceItemSlot;
    }
}
