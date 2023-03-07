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
            if (patchNote != null)
                await RespondAsync(embed: patchNote.Embed.CreateDiscordEmbed());
            else
                await RespondAsync($"Could not find a patch note numbered **{number}**");
        }

        [SlashCommand("item", "NullReferenceException Talisman")]
        public async Task PatchItem([Summary(description: "The item's name to lookup")][Autocomplete(typeof(ItemAutocompleteHandler))] string name,
                                    [Summary(description: "The specific patch to lookup")][Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null,
                                    [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            var embeds = await GetEntityPatchNotesEmbeds<ItemPatchNoteEmbed>(name, patch, locale, 3);
            if (!embeds.Any())
            {
                await RespondAsync($"No changes for this item in Patch **{patch}**", ephemeral: true);
                return;
            }
            await RespondAsync(embeds: embeds.Reverse().ToArray());
        }

        [SlashCommand("hero", "🎶 I need a hero 🎶")]
        public async Task PatchHero([Summary(description: "The heroes name to lookup")][Autocomplete(typeof(HeroAutocompleteHandler))] string name,
                                    [Summary(description: "The specific patch to lookup")][Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null,
                                    [Summary(description: "The language/locale of the response")][Autocomplete(typeof(LocaleAutocompleteHandler))] string? locale = null)
        {
            var embeds = await GetEntityPatchNotesEmbeds<HeroPatchNoteEmbed>(name, patch, locale);
            if (!embeds.Any())
            {
                await RespondAsync($"No changes for this hero in Patch **{patch}**", ephemeral: true);
                return;
            }
            await RespondAsync(embeds: embeds.ToArray());
        }

        private async Task<IEnumerable<Discord.Embed>> GetEntityPatchNotesEmbeds<T>(string name, string? patch = null, string? locale = null, int limit = 1) where T : EntityPatchNoteEmbed
        {
            var patchNotes = new List<T>();
            if (patch == null)
            {
                patchNotes.AddRange(await _db.GetPatchNotes<T>(name, locale ?? Context.Interaction.UserLocale, limit: limit, orderByDesc: true));
            }
            else
            {
                var patchnote = await _db.GetPatchNote<T>(patch, name, locale ?? Context.Interaction.UserLocale);
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
