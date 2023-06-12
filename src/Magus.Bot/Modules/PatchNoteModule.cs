using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Extensions;
using Magus.Bot.Services;
using Magus.Data;
using Magus.Data.Models.Embeds;

namespace Magus.Bot.Modules
{
    [Group("patch", "Get specific patch notes for any patch! ... *since 7.06d*.")]
    [ModuleRegistration(Location.GLOBAL)]
    public class PatchNoteModule : ModuleBase
    {
        private readonly IAsyncDataService _db;
        private readonly LocalisationService _localisationService;

        public PatchNoteModule(IAsyncDataService db, LocalisationService localisationService)
        {
            _db = db;
            _localisationService = localisationService;
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
            number ??= (await _db.GetLatestPatch()).PatchNumber;
            var patchNote = await _db.GetGeneralPatchNote(number, locale);

            if (patchNote != null)
                await FollowupAsync(embed: patchNote.Embed.CreateDiscordEmbed());
            else
                await FollowupAsync($"Could not find a patch note numbered **{number}**.");
        }

        [SlashCommand("item", "Get an item's last few patch notes.")]
        public async Task PatchItem([Summary(description: "The item's name to lookup")][Autocomplete(typeof(ItemAutocompleteHandler))] string name,
                                    [Summary(description: "The specific patch to lookup")][Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null,
                                    [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            await DeferAsync();
            locale = _localisationService.LocaleConfirmOrDefault(locale ?? Context.Interaction.UserLocale);
            var embeds = await GetEntityPatchNotesEmbeds<ItemPatchNoteEmbed>(name, patch, locale, 3);
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
            locale = _localisationService.LocaleConfirmOrDefault(locale ?? Context.Interaction.UserLocale);
            var embeds = await GetEntityPatchNotesEmbeds<HeroPatchNoteEmbed>(name, patch, locale);
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

        private async Task<IEnumerable<Discord.Embed>> GetEntityPatchNotesEmbeds<T>(string name, string? patch = null, string? locale = null, int limit = 1) where T : EntityPatchNoteEmbed
        {
            locale = _localisationService.LocaleConfirmOrDefault(locale ?? Context.Interaction.UserLocale);
            var patchNotes = new List<T>();
            if (patch == null)
            {
                patchNotes.AddRange(await _db.GetPatchNotes<T>(name, locale, limit: limit, orderByDesc: true));
            }
            else
            {
                var patchnote = await _db.GetPatchNote<T>(patch, name, locale);
                if (patchnote != null)
                    patchNotes.Add(patchnote); ;
            }
            var embeds = new List<Discord.Embed>();
            foreach (var patchNote in patchNotes)
                embeds.Add(patchNote.Embed.CreateDiscordEmbed());

            return embeds;
        }
    }
}
