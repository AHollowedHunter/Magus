using Discord.Interactions;
using Magus.Bot.Attributes;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Extensions;
using Magus.Data;
using Magus.Data.Models.Embeds;

namespace Magus.Bot.Modules
{
    [Group("patch", "Knowledge 📚")]
    [ModuleRegistration(Location.GLOBAL)]
    public class PatchNoteModule : ModuleBase
    {
        private readonly IAsyncDataService _db;

        public PatchNoteModule(IAsyncDataService db)
        {
            _db = db;
        }

        [SlashCommand("notes", "Knowledge 📚")]
        public async Task PatchNotes([Summary(description: "The specific patch to lookup")][Autocomplete(typeof(PatchAutocompleteHandler))] string number,
                                     [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            var patchNote = await _db.GetGeneralPatchNote(number, locale ?? Context.Interaction.UserLocale);
            if (patchNote == null)
            {
                await RespondAsync($"Could not find a patch note numbered {number}");
                return;
            }
            await RespondAsync(embed: patchNote.Embed.CreateDiscordEmbed());
        }

        [SlashCommand("item", "NullReferenceException Talisman")]
        public async Task PatchItem([Summary(description: "The item's name to lookup")][Autocomplete(typeof(ItemAutocompleteHandler))] string name,
                                    [Summary(description: "The specific patch to lookup")][Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null,
                                    [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            var embeds = new List<Discord.Embed>();
            IEnumerable<ItemPatchNoteEmbed> patchNotes;
            if (patch == null)
            {
                patchNotes = await _db.GetPatchNotes<ItemPatchNoteEmbed>(name, locale ?? Context.Interaction.UserLocale, limit: 3, orderByDesc: true);
            }
            else
            {
                patchNotes = new List<ItemPatchNoteEmbed> { await _db.GetPatchNote<ItemPatchNoteEmbed>(patch, name, locale ?? Context.Interaction.UserLocale) };
            }

            if (patchNotes == null || patchNotes.Any(x => x == null) || !patchNotes.Any())
            {
                await RespondAsync($"No changes for this item in patch {patch}", ephemeral: true);
                return;
            }
            foreach (var patchNote in patchNotes)
            {
                embeds.Add(patchNote.Embed.CreateDiscordEmbed());
            }
            embeds.Reverse();

            await RespondAsync(embeds: embeds.ToArray());
        }

        [SlashCommand("hero", "🎶 I need a hero 🎶")]
        public async Task PatchHero([Summary(description: "The heroes name to lookup")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
                                    [Summary(description: "The specific patch to lookup")][Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null,
                                    [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            IEnumerable<HeroPatchNoteEmbed> patchNotes;
            if (patch == null)
            {
                patchNotes = await _db.GetPatchNotes<HeroPatchNoteEmbed>(name, locale ?? Context.Interaction.UserLocale, limit: 1, orderByDesc: true);
            }
            else
            {
                var patchNote = new List<HeroPatchNoteEmbed>
                {
                    await _db.GetPatchNote<HeroPatchNoteEmbed>(patch, name, locale ?? Context.Interaction.UserLocale)
                };
                patchNotes = patchNote;
            }

            if (patchNotes == null || patchNotes.Any(x => x == null) || !patchNotes.Any())
            {
                await RespondAsync("Could not find any changes for this hero.", ephemeral: true);
                return;
            }

            var embeds = new List<Discord.Embed>();
            foreach (var patchNote in patchNotes)
            {
                embeds.Add(patchNote.Embed.CreateDiscordEmbed());
            }

            await RespondAsync(embeds: embeds.ToArray());
        }
    }
}
