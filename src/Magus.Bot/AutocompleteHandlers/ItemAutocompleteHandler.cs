using Magus.Data.Enums;
using Magus.Data.Services;

namespace Magus.Bot.AutocompleteHandlers;

public sealed class ItemAutocompleteHandler : EntityAutocompleteHandler
{
    internal override EntityType EntityType => EntityType.Item;

    public ItemAutocompleteHandler(MeilisearchService meilisearch) : base(meilisearch) { }
}
