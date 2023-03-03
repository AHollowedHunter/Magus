using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Magus.Bot.Attributes;
using System.Reflection;

namespace Magus.Bot
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly ILogger<InteractionHandler> _logger;

        public InteractionHandler(DiscordSocketClient client, InteractionService interactions, ILogger<InteractionHandler> logger, IServiceProvider services)
        {
            _client       = client;
            _interactions = interactions;
            _logger       = logger;
            _services     = services;
        }

        public async Task InitializeAsync()
        {
            foreach (var module in GetEnabledModules())
                await _interactions.AddModuleAsync(module, _services);

            _client.InteractionCreated += HandleInteraction;

            // Process the command execution results 
            _interactions.SlashCommandExecuted     += SlashCommandExecuted;
            _interactions.ContextCommandExecuted   += ContextCommandExecuted;
            _interactions.ComponentCommandExecuted += ComponentCommandExecuted;
            _interactions.ModalCommandExecuted     += ModalCommandExecuted;

            _logger.LogInformation("InteractionHandler Initialised, {count} Modules registered: {moudleList}", _interactions.Modules.Count, string.Join(", ", _interactions.Modules.Select(x => x.Name)));
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
            if (interaction.Type == InteractionType.MessageComponent)
                _logger.LogDebug("CustomId: {id}", ((SocketMessageComponent)interaction).Data.CustomId);

            try
            {
                var ctx = new SocketInteractionContext(_client, interaction);

                await _interactions.ExecuteCommandAsync(ctx, _services);
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

        private Task ComponentCommandExecuted(ComponentCommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
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

        private Task ContextCommandExecuted(ContextCommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
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

        private Task SlashCommandExecuted(SlashCommandInfo slashCommandInfo, IInteractionContext interactionContext, IResult result)
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
                        _logger.LogError("Error handling slash command");
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

        private Task ModalCommandExecuted(ModalCommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
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