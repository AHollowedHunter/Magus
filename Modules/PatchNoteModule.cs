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
            Discord.Embed embed;
            IEnumerable<ItemPatchNote> patchNotes;
            if (patch == null)
            {
                patchNotes = _db.GetPatchNotes<ItemPatchNote>(id, limit: 3, orderByDesc: true);
            }
            else
            {
                patchNotes = new List<ItemPatchNote> { _db.GetPatchNote<ItemPatchNote>(patch, id) };
            }

            if (patchNotes == null || patchNotes.Any(x => x == null) || patchNotes.Count() == 0)
            {
                await RespondAsync($"No changes for this item in patch {patch}", ephemeral: true);
                return;
            }
            else if (patchNotes.Count() == 1)
            {
                embed = patchNotes.First().Embed.CreateDiscordEmbed();
            }
            else
            {
                var firstEmbed = patchNotes.First().Embed;
                var embedBuilder = new EmbedBuilder
                {
                    Title = $"{patchNotes.First().LocalName} recent changes",
                    ThumbnailUrl = firstEmbed.ThumbnailUrl,
                    Color = firstEmbed.ColorRaw,
                    Timestamp = firstEmbed.Timestamp,
                    Footer = new() { Text = firstEmbed.Footer?.Text, IconUrl = firstEmbed.Footer?.IconUrl }
                };
                foreach (var patchNote in patchNotes)
                {
                    embedBuilder.AddField(new EmbedFieldBuilder() { Name = $"Patch {patchNote.PatchNumber}", Value = patchNote.Embed.Description });
                }
                embed = embedBuilder.Build();
            }
            await RespondAsync(embed: embed);
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
