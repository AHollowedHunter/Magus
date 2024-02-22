using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Services;
using Magus.Data.Enums;
using Magus.Data.Services;

namespace Magus.Bot.Modules;

[Group("patch", "Get specific patch notes for any patch! ... *since 7.06d*.")]
[ModuleRegistration(Location.GLOBAL)]
public class PatchNoteModule : ModuleBase
{
    private readonly LocalisationService _localisationService;
    private readonly MeilisearchService _meilisearchService;

    public PatchNoteModule(LocalisationService localisationService, MeilisearchService meilisearchService)
    {
        _localisationService = localisationService;
        _meilisearchService = meilisearchService;
    }

    [SlashCommand("when", "how long is a piece of string?")]
    public async Task When()
        => await RespondAsync("After a new patch is announced, it may take around ~30-60 minutes for me to fully update depending on different factors.\nIf they make breaking changes in game files it will take longer.\nIf they patch after midnight UTC... I'm sleeping 😅.");

    [SlashCommand("notes", "Get General Patch notes.")]
    public async Task PatchNotes([Summary(description: "The specific patch to lookup. Otherwise defaults to latest.")][Autocomplete(typeof(PatchAutocompleteHandler))] string? number = null,
                                 [Summary(description: "The language/locale of the response.")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
    {
        await DeferAsync();
        locale = _localisationService.LocaleConfirmOrDefault(locale ?? Context.Interaction.UserLocale);
        number ??= (await _meilisearchService.GetLatestPatchAsync().ConfigureAwait(false)).PatchNumber;
        var patchNotes = await _meilisearchService.SearchPatchNotesAsync(null, number, PatchNoteType.General, locale, 1).ConfigureAwait(false);

        if (patchNotes != null)
            await FollowupAsync(embed: patchNotes.Single().Embed.ToDiscordEmbed());
        else
            await FollowupAsync($"Could not find a patch note numbered **{number}**.");
    }

    [SlashCommand("item", "Get an item's last few patch notes.")]
    public async Task PatchItem([Summary(description: "The item's name to lookup")][Autocomplete(typeof(ItemAutocompleteHandler))] string name,
                                [Summary(description: "The specific patch to lookup")][Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null,
                                [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
    {
        await DeferAsync();
        var embeds = await GetEntityPatchNotesEmbeds(name, patch, PatchNoteType.Item, locale, 3);
        if (!embeds.Any())
        {
            if (patch != null)
                await FollowupAsync($"No changes for this item in Patch **{patch}**.", ephemeral: true);
            else
                await FollowupAsync($"No patch notes for this item.", ephemeral: true);
            return;
        }
        await FollowupAsync(embeds: embeds.Reverse().ToArray());
    }

    [SlashCommand("hero", "Get the latest patch note for a hero.")]
    public async Task PatchHero([Summary(description: "The heroes name to lookup")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
                                [Summary(description: "The specific patch to lookup")][Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null,
                                [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
    {
        await DeferAsync();
        var embeds = await GetEntityPatchNotesEmbeds(name, patch, PatchNoteType.Hero, locale);
        if (!embeds.Any())
        {
            if (patch != null)
                await FollowupAsync($"No changes for this hero in Patch **{patch}**.", ephemeral: true);
            else
                await FollowupAsync($"No patch notes for this hero.", ephemeral: true);
            return;
        }
        await FollowupAsync(embeds: embeds.ToArray());
    }

    private async Task<IEnumerable<Discord.Embed>> GetEntityPatchNotesEmbeds(string name, string? patch = null, PatchNoteType? type = null, string? locale = null, int limit = 1)
    {
        locale = _localisationService.LocaleConfirmOrDefault(locale ?? Context.Interaction.UserLocale);
        var patchNotes = await _meilisearchService.SearchPatchNotesAsync(name, patch, type, locale, limit).ConfigureAwait(false); // TODO ensure only gets hero

        var embeds = new List<Discord.Embed>();
        foreach (var patchNote in patchNotes)
            embeds.Add(patchNote.Embed.ToDiscordEmbed());

        return embeds;
    }
}
