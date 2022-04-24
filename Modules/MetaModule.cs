using Discord;
using Discord.Interactions;
using Magus.Data;

namespace Magus.Bot.Modules
{
    [Group("magus", "meta commands")]
    public class MetaModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IDatabaseService _db;
        private readonly IServiceProvider _services;

        public MetaModule(IDatabaseService db, IServiceProvider services)
        {
            _db = db;
            _services = services;
        }

        string version = "v1.0.0";
        DateTimeOffset versionDate = DateTimeOffset.Now;

        [SlashCommand("about", "About this bot ℹ")]
        public async Task About()
        {
            var author = await Context.Client.GetUserAsync(240463688627126278);
            var latestPatchNote = _db.GetLatestPatch();
            var latestPatch = $"[{ latestPatchNote.PatchNumber }](https://www.dota2.com/patches/{latestPatchNote.PatchNumber}) - <t:{latestPatchNote.PatchTimestamp}:R>";
            var response = new EmbedBuilder()
            {
                Title = "MagusBot",
                Description = "A DotA 2 Discord bot",
                Author = new EmbedAuthorBuilder() { Name = "AHollowedHunter", Url = $"https://github.com/AHollowedHunter", IconUrl = author.GetAvatarUrl() },
                Color = Color.Purple,
                Timestamp = versionDate,
                Footer = new() { Text = "Hot Damn!", IconUrl = Context.Client.CurrentUser.GetAvatarUrl() },
            };
            response.AddField(new EmbedFieldBuilder() { Name = "Version", Value = version, IsInline = true });
            response.AddField(new EmbedFieldBuilder() { Name = "Latest Patch", Value = latestPatch, IsInline = true });
            response.AddField(new EmbedFieldBuilder() { Name = "Total Guilds", Value = Context.Client.Guilds.Count(), IsInline = false });
            await RespondAsync(embed: response.Build(), ephemeral: true);
        }

        [SlashCommand("help", "NUJV")]
        public async Task Help()
        {
            var response = new EmbedBuilder()
            {
                Title = "MagusBot Commands",
                Description = "A list of commands for MagusBot.\n" +
                "This bot uses '/' slash commands, please type a slash '/' and use the menu that appears to select a command.\n" +
                "Alternatively, a list of all commands are provided below.\n" +
                "Required parameters are surrounded with '< >' and optional with '[ ]'",
                Color = Color.DarkGreen,
                Timestamp = versionDate,
                Footer = new() { Text = $"MagusBot version {version}"}
            };

            List<IApplicationCommand> commands = new List<IApplicationCommand>();
            commands.AddRange(await Context.Guild.GetApplicationCommandsAsync());
            commands.AddRange(await Context.Client.Rest.GetGlobalApplicationCommands());

            foreach (IApplicationCommand command in commands)
            {
                var field = new EmbedFieldBuilder()
                {
                    Name = command.Name,
                };
                var value = "";
                foreach (var option in command.Options)
                {
                    value += $"```md\n/{command.Name} {option.Name} ";
                    foreach (var param in option.Options)
                    {
                        if (param.IsRequired != null && param.IsRequired == true)
                        {
                            value += $"<{param.Name}:> ";
                        }
                        else
                        {
                            value += $"[{param.Name}:] ";
                        }
                    }
                    value += "```";
                }
                field.WithValue(value);
                response.AddField(field);
            }
            await RespondAsync(embed: response.Build(), ephemeral: true);
        }
    }
}
