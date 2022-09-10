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
        private readonly IServiceProvider _services;

        public PatchNoteModule(IAsyncDataService db, IServiceProvider services)
        {
            _db = db;
            _services = services;
        }

        [SlashCommand("notes", "Knowledge 📚")]
        public async Task PatchNotes([Autocomplete(typeof(PatchAutocompleteHandler))] string number)
        {
            var patchNote = await _db.GetGeneralPatchNote(number);
            if (patchNote == null)
            {
                await RespondAsync($"Could not find a patch note numbered {number}");
                return;
            }
            await RespondAsync(embed: patchNote.Embed.CreateDiscordEmbed());
        }

        [SlashCommand("item", "NullReferenceException Talisman")]
        public async Task PatchItem(
            [Autocomplete(typeof(ItemAutocompleteHandler))] string name,
            [Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null)
        {
            var embeds = new List<Discord.Embed>();
            IEnumerable<ItemPatchNoteEmbed> patchNotes;
            if (patch == null)
            {
                patchNotes = await _db.GetPatchNotes<ItemPatchNoteEmbed>(name, Context.Interaction.UserLocale, limit: 3, orderByDesc: true);
            }
            else
            {
                patchNotes = new List<ItemPatchNoteEmbed> { await _db.GetPatchNote<ItemPatchNoteEmbed>(patch, name, Context.Interaction.UserLocale) };
            }

            if (patchNotes == null || patchNotes.Any(x => x == null) || patchNotes.Count() == 0)
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
        public async Task PatchHero(
            [Autocomplete(typeof(HeroAutocompleteHandler))] string name,
            [Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null)
        {
            IEnumerable<HeroPatchNoteEmbed> patchNotes;
            if (patch == null)
            {
                patchNotes = await _db.GetPatchNotes<HeroPatchNoteEmbed>(name, Context.Interaction.UserLocale, limit: 1, orderByDesc: true);
            }
            else
            {
                var patchNote = new List<HeroPatchNoteEmbed>();
                patchNote.Add(await _db.GetPatchNote<HeroPatchNoteEmbed>(patch, name, Context.Interaction.UserLocale));
                patchNotes = patchNote;
            }

            if (patchNotes == null || patchNotes.Any(x => x == null) || patchNotes.Count() == 0)
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
