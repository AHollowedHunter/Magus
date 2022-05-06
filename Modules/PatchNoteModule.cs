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
            var patchNote = _db.GetGeneralPatchNote(number).Embed;

            var response = patchNote.CreateDiscordEmbed();

            await RespondAsync(embed: response);
        }

        [SlashCommand("item", "NullReferenceException Talisman")]
        public async Task PatchItem(
            [Autocomplete(typeof(ItemAutocompleteHandler))] int id,
            [Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null)
        {
            IEnumerable<ItemPatchNote> patchNotes;
            if (patch == null)
            {
                patchNotes = _db.GetPatchNotes<ItemPatchNote>(id, limit: 1, orderByDesc: true);
            }
            else
            {
                var patchNote = new List<ItemPatchNote>();
                patchNote.Add(_db.GetPatchNote<ItemPatchNote>(patch, id));
                patchNotes = patchNote;
            }

            if (patchNotes.Count() == 0)
            {
                await RespondAsync("Could not find any changes for this item.", ephemeral: true);
                return;
            }

            var embeds = new List<Discord.Embed>();
            foreach (var patchNote in patchNotes)
            {
                embeds.Add(patchNote.Embed.CreateDiscordEmbed());
            }

            await RespondAsync(embeds: embeds.ToArray());
        }

        [SlashCommand("hero", "🎶 I need a hero 🎶")]
        public async Task PatchHero(
            [Autocomplete(typeof(HeroAutocompleteHandler))] int id,
            [Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null)
        {
            IEnumerable<HeroPatchNote> patchNotes;
            if (patch == null)
            {
                patchNotes = _db.GetPatchNotes<HeroPatchNote>(id, limit: 1, orderByDesc: true);
            }
            else
            {
                var patchNote = new List<HeroPatchNote>();
                patchNote.Add(_db.GetPatchNote<HeroPatchNote>(patch, id));
                patchNotes = patchNote;
            }

            if (patchNotes.Count() == 0)
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
