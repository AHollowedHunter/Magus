using Discord;
using Discord.Interactions;
using Magus.Bot.AutocompleteHandlers;
using Magus.Bot.Extensions;
using Magus.Data;
using Magus.Data.Models;
using Magus.Data.Models.Embeds;

namespace Magus.Bot.Modules
{
    public class PatchNoteModule : InteractionModuleBase<SocketInteractionContext>
    {

        //[RequireBotPermission(ChannelPermission.SendMessages)]
        [Group("patch", "Knowledge 📚")]
        public class PatchGroup : InteractionModuleBase<SocketInteractionContext>
        {
            private readonly IDatabaseService _db;
            private readonly IServiceProvider _services;

            public PatchGroup(IDatabaseService db, IServiceProvider services)
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
                [Autocomplete(typeof(ItemAutocompleteHandler))] string id,
                [Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null)
            {
                if (!int.TryParse(id, System.Globalization.NumberStyles.Integer, null, out int itemId))
                {
                    try
                    {
                        itemId = (int)_db.GetEntityInfo<ItemInfo>(id, limit: 1).First().Id;
                    }
                    catch
                    {
                        await RespondAsync("Error parsing id.", ephemeral: true);
                        return;
                    }
                }

                IEnumerable<ItemPatchNote> patchNotes;
                if (patch == null)
                {
                    patchNotes = _db.GetPatchNotes<ItemPatchNote>(itemId, limit: 1, orderByDesc: true);
                }
                else
                {
                    var patchNote = new List<ItemPatchNote>();
                    patchNote.Add(_db.GetPatchNote<ItemPatchNote>(patch, itemId));
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
                [Autocomplete(typeof(HeroAutocompleteHandler))] string id,
                [Autocomplete(typeof(PatchAutocompleteHandler))] string? patch = null)
            {
                if (!int.TryParse(id, System.Globalization.NumberStyles.Integer, null, out int heroId))
                {
                    try
                    {
                        heroId = (int)_db.GetEntityInfo<HeroInfo>(id, limit: 1).First().Id;
                    }
                    catch
                    {
                        await RespondAsync("Error parsing id.", ephemeral: true);
                        return;
                    }
                }

                IEnumerable<HeroPatchNote> patchNotes;
                if (patch == null)
                {
                    patchNotes = _db.GetPatchNotes<HeroPatchNote>(heroId, limit: 1, orderByDesc: true);
                }
                else
                {
                    var patchNote = new List<HeroPatchNote>();
                    patchNote.Add(_db.GetPatchNote<HeroPatchNote>(patch, heroId));
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
}
