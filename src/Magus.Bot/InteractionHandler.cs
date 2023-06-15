using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Magus.Bot.Attributes;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Magus.Bot
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _services;
        private readonly ILogger<InteractionHandler> _logger;
        private readonly BotSettings _botSettings;

        public InteractionHandler(DiscordSocketClient client, InteractionService interactions, ILogger<InteractionHandler> logger, IServiceProvider services, IOptions<BotSettings> botSettings)
        {
            _client = client;
            _interactionService = interactions;
            _logger = logger;
            _services = services;
            _botSettings = botSettings.Value;
        }

        public async Task InitialiseAsync()
        {
            foreach (var module in GetEnabledModules())
                await _interactionService.AddModuleAsync(module, _services);

            _client.InteractionCreated += HandleInteraction;

            // Process the command execution results 
            _interactionService.SlashCommandExecuted += SlashCommandExecuted;
            _interactionService.ContextCommandExecuted += ContextCommandExecuted;
            _interactionService.ComponentCommandExecuted += ComponentCommandExecuted;
            _interactionService.ModalCommandExecuted += ModalCommandExecuted;

            _logger.LogInformation("InteractionHandler Initialised, {count} Modules registered: {moduleList}", _interactionService.Modules.Count, string.Join(", ", _interactionService.Modules.Select(x => x.Name)));
        }

        public async Task RegisterModulesAsync()
        {
            _logger.LogInformation("Registering Modules");
            var modules = new Dictionary<ModuleInfo, Location>();
            foreach (var module in _interactionService.Modules)
                if (module.IsTopLevelGroup || !module.IsSlashGroup) // We only need the top-level group, or non-group modules. Exclude sub-groups.
                    modules.Add(module, ((ModuleRegistration)module.Attributes.First(x => typeof(ModuleRegistration).IsAssignableFrom(x.GetType()))).Location);

            // Register GLOBAL commands
            var globalModules = modules.Where(x => x.Value == Location.GLOBAL).Select(x => x.Key).ToArray();
            await _interactionService.AddModulesGloballyAsync(true, modules: globalModules);

            // Disabled, need to fix registering to the same guild multiple times. Either create logic to remove commands separately, or get all modules applicable to a guild 
            // What happens when removing a guild? need to de-register the commands
            // Maybe a DB field with guild "CommandsLastRegistered" and whenever the bot updates cycle through? 
            // 
            // Register TESTING commands
            //foreach (var guild in configuration.GetSection("TestingGuilds").Get<ulong[]>())
            //    await _interactionService.AddModulesToGuildAsync(guild, true, modules: modules.Where(x => x.Value == Location.TESTING).Select(x => x.Key).ToArray());

            // Register MANAGEMENT commands
            var managementModules = modules.Where(x => x.Value == Location.MANAGEMENT).Select(x => x.Key).ToArray();
            foreach (var guildId in _botSettings.ManagementGuilds)
                await _interactionService.AddModulesToGuildAsync(guildId, true, modules: managementModules);

            _logger.LogInformation("Complete Module Registration");
        }

        private List<TypeInfo> GetEnabledModules()
        {
            TypeInfo moduleTypeInfo = typeof(IInteractionModuleBase).GetTypeInfo();
            List<TypeInfo> result   = new();
            foreach (TypeInfo definedType in Assembly.GetEntryAssembly()!.DefinedTypes)
            {
                var moduleRegistration = definedType.GetCustomAttribute<ModuleRegistration>();
                if (!(moduleRegistration != null && moduleRegistration.IsEnabled))
                    continue;

                if (moduleTypeInfo.IsAssignableFrom(definedType))
                    result.Add(definedType);
                else
                    _logger.LogWarning("Class {name} is marked with ModuleRegistration, but it does not implement IInteractionModuleBase", definedType.FullName);
            }
            return result;
        }

        # region Execution

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                var ctx = new SocketInteractionContext(_client, interaction);

                await _interactionService.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling interaction");

                // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (interaction.Type == InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        # endregion

        # region Error Handling

        private Task ComponentCommandExecuted(ComponentCommandInfo commandInfo, IInteractionContext ctx, IResult result)
        {
            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // implement
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        break;
                    case InteractionCommandError.Exception:
                        // implement
                        break;
                    case InteractionCommandError.Unsuccessful:
                        // implement
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private Task ContextCommandExecuted(ContextCommandInfo commandInfo, IInteractionContext ctx, IResult result)
        {
            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // implement
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        break;
                    case InteractionCommandError.Exception:
                        // implement
                        break;
                    case InteractionCommandError.Unsuccessful:
                        // implement
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private Task SlashCommandExecuted(SlashCommandInfo commandInfo, IInteractionContext ctx, IResult result)
        {
            var nameArray = commandInfo.ToString().Split(' ');
            var labels = new string[] {
                nameArray[0],
                nameArray.Length == 3 ? nameArray[1] : "",
                nameArray.Length > 1 ? nameArray.Last() : "",
                result.IsSuccess.ToString(),
                result.Error.ToString() ?? ""
            };
            MagusMetrics.SlashCommandsExecuted.WithLabels(labels).Inc();

            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // implement
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        break;
                    case InteractionCommandError.Exception:
                        _logger.LogError("Error Executing Slash Command: {reason}", result.ErrorReason);
                        ctx.Interaction.FollowupAsync("Apologies, there was a problem while processing this command!\n\n" +
                            "Please try again, and if it persists you can search or report it on [GitHub](<https://github.com/AHollowedHunter/Magus/issues/>) and provide the command you had the problem with.");
                        break;
                    case InteractionCommandError.Unsuccessful:
                        // implement
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private Task ModalCommandExecuted(ModalCommandInfo commandInfo, IInteractionContext ctx, IResult result)
        {
            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        // implement
                        break;
                    case InteractionCommandError.BadArgs:
                        // implement
                        break;
                    case InteractionCommandError.Exception:
                        // implement
                        break;
                    case InteractionCommandError.Unsuccessful:
                        // implement
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }
        # endregion
    }
}