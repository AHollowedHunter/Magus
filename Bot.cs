using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Magus.Bot.Attributes;
using Magus.Bot.Services;
using Magus.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Magus.Bot
{
    class Bot
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "MAGUS_")
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets<Bot>()
                .Build();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private static ILogger<Bot> _logger;
        private static InteractionService _interactionService;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }

        static async Task RunAsync()
        {
            using var services  = ConfigureServices();
            var client          = services.GetRequiredService<DiscordSocketClient>();
            _interactionService = services.GetRequiredService<InteractionService>();
            _logger             = services.GetRequiredService<ILogger<Bot>>();

            client.Log              += LogAsync;
            _interactionService.Log += LogAsync;
            
            if (IsDebug())
                client.Ready += async () => await _interactionService.RegisterCommandsToGuildAsync(configuration.GetValue<ulong>("DevGuild"), true);
            else
                client.Ready += RegisterModules;

            // Here we can initialize the service that will register and execute our commands
            await services.GetRequiredService<CommandHandler>().InitializeAsync();

            await client.LoginAsync(TokenType.Bot, configuration["BotToken"]);
            await client.StartAsync();

            await client.SetGameAsync(name: "\"/magus help\"", type: ActivityType.Playing);

            await Task.Delay(Timeout.Infinite);
        }

        static async Task RegisterModules()
        {
            var modules = new Dictionary<ModuleInfo, Location>();
            foreach (var module in _interactionService.Modules)
                modules.Add(module, ((ModuleRegistration)module.Attributes.First(x => typeof(ModuleRegistration).IsAssignableFrom(x.GetType()))).Location);

            // Register GLOBAL commands
            await _interactionService.AddModulesGloballyAsync(true, modules: modules.Where(x => x.Value == Location.GLOBAL).Select(x => x.Key).ToArray());

            // Disabled, need to fix registering to the same guild multiple times. Either create logic to remove commands separately, or get all modules applicable to a guild 
            // What happens when removing a guild? need to de-register the commands
            // Maybe a DB field with guild "CommandsLastRegistered" and whenever the bot updates cycle through? 
            // 
            // Register TESTING commands
            //foreach (var guild in configuration.GetSection("TestingGuilds").Get<ulong[]>())
            //    await _interactionService.AddModulesToGuildAsync(guild, true, modules: modules.Where(x => x.Value == Location.TESTING).Select(x => x.Key).ToArray());

            // Register MANAGEMENT commands
            foreach (var guild in configuration.GetSection("ManagementGuilds").Get<ulong[]>())
                await _interactionService.AddModulesToGuildAsync(guild, true, modules: modules.Where(x => x.Value == Location.MANAGEMENT).Select(x => x.Key).ToArray());

            return;
        }

        static Task LogAsync(LogMessage message)
        {
            var severity = message.Severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error    => LogLevel.Error,
                LogSeverity.Warning  => LogLevel.Warning,
                LogSeverity.Info     => LogLevel.Information,
                LogSeverity.Verbose  => LogLevel.Debug,
                LogSeverity.Debug    => LogLevel.Trace,
                _                    => LogLevel.Information
            };
            _logger.Log(severity, message.Exception, $"[{{0}}] {message.Message ?? message.Exception.Message}", message.Source);
            return Task.CompletedTask;
        }

        static ServiceProvider ConfigureServices()
            => new ServiceCollection()
                .AddLogging(x => x.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger()))
                .AddSingleton(configuration)
                .AddSingleton<IDatabaseService>(x => new LiteDBService(configuration.GetSection("DatabaseService")))
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.AllUnprivileged }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
                .AddSingleton<Services.IWebhook>(x => new DiscordWebhook())
                .BuildServiceProvider();

        static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}