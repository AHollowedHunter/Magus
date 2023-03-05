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
    /// <remarks>
    /// Will cause issues if not used inconjunction with Group Attribute
    /// See https://github.com/AHollowedHunter/Magus/issues/1
    /// </remarks>
    public abstract class ModuleBase : InteractionModuleBase<SocketInteractionContext> // InteractionService will log a warning "not public" (as of v3.8) as the class is abstract. Ignore
    {
        static readonly string version = Assembly.GetEntryAssembly()!.GetName().Version!.ToString();

        protected ModuleBase()
        {

        }

        [SlashCommand("help", "Get help with these commands")]
        public async Task Help()
        {
            await RespondAsync(embed: await CreateHelpEmbed(Context, GetType()), ephemeral: true);
        }

        internal static async Task<Embed> CreateHelpEmbed(SocketInteractionContext context, Type type)
        {
            List<IApplicationCommand> commands = new();
            commands.AddRange(await context.Client.Rest.GetGlobalApplicationCommands());
            if (!context.Interaction.IsDMInteraction)
                commands.AddRange(await context.Guild.GetApplicationCommandsAsync());

            var embeds = new List<Embed>();

            var groupAttribute = type.GetCustomAttribute<GroupAttribute>();
            var command = commands.FirstOrDefault(x => x.Name == groupAttribute?.Name);

            // If used in a subcommand class this will need to reflect the declaring type for the command name
            if (command == null)
            {
                var declaringType = type.DeclaringType;
                var baseAttribute = declaringType?.GetCustomAttributes(typeof(GroupAttribute), true)
                                          .FirstOrDefault() as GroupAttribute;
                command = commands.FirstOrDefault(x => x.Name == baseAttribute?.Name);
            }

            var embed = new EmbedBuilder()
            {
                Title = "/" + command!.Name,
                Description = command.Description,
                Color = Color.DarkGreen,
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
                    embed.AddField($"/{command.Name} {option.Name}", value);
                }
            }
            return embed.Build();
        }
    }
}
