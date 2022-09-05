using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Magus.Bot.Attributes;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Magus.Bot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly ILogger<CommandHandler> _logger;

        public CommandHandler(DiscordSocketClient client, InteractionService interactions, ILogger<CommandHandler> logger, IServiceProvider services)
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

            _logger.LogInformation("CommandHandler Initialised");
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
                    _logger.LogWarning("Class {0} is marked with ModuleRegistration, but it does not implement IInteractionModuleBase", definedType.FullName);
            }
            return result;
        }

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

        # region Execution

        private async Task HandleInteraction(SocketInteraction interaction)
        {
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
    }
}