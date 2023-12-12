using Magus.Data.Enums;
using Magus.Data.Services;

namespace Magus.Bot.AutocompleteHandlers;

public sealed class HeroAutocompleteHandler : EntityAutocompleteHandler
{
    internal override EntityType EntityType => EntityType.Hero;

    public HeroAutocompleteHandler(MeilisearchService meilisearch) : base(meilisearch) { }
}
