using Magus.Data.Enums;
using Magus.Data.Services;

namespace Magus.Bot.AutocompleteHandlers;

public sealed class AbilityAutocompleteHandler : EntityAutocompleteHandler
{
    internal override EntityType EntityType => EntityType.Ability;

    public AbilityAutocompleteHandler(MeilisearchService meilisearch) : base(meilisearch) { }
}

