using Discord;
using Discord.Interactions;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Extensions;
using Magus.Data;
using Magus.Data.Models.Embeds;

namespace Magus.Bot.Modules
{
    [Group("patch", "Knowledge 📚")]
    public class PatchNoteModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IDatabaseService _db;
        private readonly IServiceProvider _services;

        public PatchNoteModule(IDatabaseService db, IServiceProvider services)
        {
            _db = db;
            _services = services;
        }

        [SlashCommand("notes", "Knowledge 📚")]
        public async Task PatchNotes([Autocomplete(typeof(PatchAutocompleteHandler))] string number)
        {
            var patchNote = _db.GetGeneralPatchNote(number);
            if (patchNote == null)
            {
                await RespondAsync($"Could not find a patch note numbered {number}");
                return;
            }
            await RespondAsync(embed: patchNote.Embed.CreateDiscordEmbed());
        }

        [SlashCommand("item", "NullReferenceException Talisman")]
        public async Task PatchItem(
            [Autocomplete(typeof(ItemAutocompleteHandler))] int id,
            [Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null)
        {
            var embeds = new List<Discord.Embed>();
            IEnumerable<ItemPatchNoteEmbed> patchNotes;
            if (patch == null)
            {
                patchNotes = _db.GetPatchNotes<ItemPatchNoteEmbed>(id, limit: 3, orderByDesc: true);
            }
            else
            {
                patchNotes = new List<ItemPatchNoteEmbed> { _db.GetPatchNote<ItemPatchNoteEmbed>(patch, id) };
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
            [Autocomplete(typeof(HeroAutocompleteHandler))] int id,
            [Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null)
        {
            IEnumerable<HeroPatchNoteEmbed> patchNotes;
            if (patch == null)
            {
                patchNotes = _db.GetPatchNotes<HeroPatchNoteEmbed>(id, limit: 1, orderByDesc: true);
            }
            else
            {
                var patchNote = new List<HeroPatchNoteEmbed>();
                patchNote.Add(_db.GetPatchNote<HeroPatchNoteEmbed>(patch, id));
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
