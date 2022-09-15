using Discord;
using Discord.Interactions;
using System.Reflection;

namespace Magus.Bot.Modules
{
    /// <summary>
    /// Using ModuleBase will use up one of the 25 subcommands available to add the '/{group} help' command
    /// 
    /// Any single root commands will need a separate root "help" command if desired
    /// </summary>
    public abstract class ModuleBase : InteractionModuleBase<SocketInteractionContext> // InteractionService will log a warning "not public" (as of v3.8) as the class is abstract. Ignore
    {
        readonly string version             = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";
        readonly DateTimeOffset versionDate = DateTimeOffset.Parse("2022-09-15T21:15:00Z"); // improve this

        protected ModuleBase()
        {

        }

        [SlashCommand("help", "Get help with these commands")]
        public async Task Help()
        {
            await RespondAsync(embed: await CreateHelpEmbed(), ephemeral: true);
        }

        private async Task<Embed> CreateHelpEmbed()
        {
            List<IApplicationCommand> commands = new();
            commands.AddRange(await Context.Client.Rest.GetGlobalApplicationCommands());
            if (!Context.Interaction.IsDMInteraction)
                commands.AddRange(await Context.Guild.GetApplicationCommandsAsync());

            var embeds = new List<Embed>();

            var groupAttribute = GetType().GetCustomAttributes(typeof(GroupAttribute), true)
                                          .FirstOrDefault() as GroupAttribute;
            var command = commands.FirstOrDefault(x => x.Name == groupAttribute?.Name);

            // If used in a subcommand class this will need to reflect the declaring type for the command name
            if (command == null)
            {
                var declaringType = GetType().DeclaringType;
                var baseAttribute = declaringType?.GetCustomAttributes(typeof(GroupAttribute), true)
                                          .FirstOrDefault() as GroupAttribute;
                command = commands.FirstOrDefault(x => x.Name == baseAttribute?.Name);
            }

            var embed = new EmbedBuilder()
            {
                Title = "/" + command!.Name,
                Description = command.Description,
                Color = Color.DarkGreen,
                Timestamp = versionDate,
                Footer = new() { Text = $"MagusBot version {version}" }
            };

            if (!command.Options.Any(x => x.Type == ApplicationCommandOptionType.SubCommand))
            {
                embed.Description += $"\nTry it: </{command.Name}:{command.Id}>\n";
            }
            else
            {
                foreach (var option in command.Options)
                {
                    var field = new EmbedFieldBuilder()
                    {
                        Name = $"/{command.Name} {option.Name}",
                    };
                    var value = $"{option.Description}\n";
                    if (option.Type == ApplicationCommandOptionType.SubCommand)
                    {
                        value += $"Try it: </{command.Name} {option.Name}:{command.Id}>\n";
                        foreach (var parameter in option.Options)
                            value += $"`{parameter.Name}` - {parameter.Description}\n";
                    }
                    else
                    {
                        foreach (var optionLevelTwo in option.Options)
                        {
                            if (optionLevelTwo.Type == ApplicationCommandOptionType.SubCommand)
                            {
                                var fullCommand = $"{command.Name} {option.Name} {optionLevelTwo.Name}";
                                value += $"**/{fullCommand}**\n*{optionLevelTwo.Description}*\nTry it: </{fullCommand}:{command.Id}>\n";
                                foreach (var parameter in optionLevelTwo.Options)
                                    value += $"`{parameter.Name}` - {parameter.Description}\n";
                            }
                        }
                    }
                    field.WithValue(value);
                    embed.AddField(field);
                }
            }
            return embed.Build();
        }
    }
}
