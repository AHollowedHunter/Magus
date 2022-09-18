using Coravel;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Magus.Bot.Attributes;
using Magus.Bot.Services;
using Magus.Common;
using Magus.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//using Magus.DataBuilder;
using Serilog;

namespace Magus.Bot
{
    class Bot
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "MAGUS_")
                .AddUserSecrets<Bot>(optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private static ILogger<Bot> _logger;
        private static InteractionService _interactionService;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        static void Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureLogging((context, logging) => logging.ClearProviders())
                .ConfigureServices((_, serviceCollection) => ConfigureServices(serviceCollection))
                .Build();

            RunAsync(host).GetAwaiter().GetResult();
        }

        static async Task RunAsync(IHost host)
        {
            var services        = host.Services;
            var client          = services.GetRequiredService<DiscordSocketClient>();
            _interactionService = services.GetRequiredService<InteractionService>();
            _logger             = services.GetRequiredService<ILogger<Bot>>();

            client.Log              += LogAsync;
            _interactionService.Log += LogAsync;

            if (IsDebug())
                client.Ready += async () => await _interactionService.RegisterCommandsToGuildAsync(configuration.GetValue<ulong>("DevGuild"), true);
            else
                client.Ready += RegisterModules;

            client.JoinedGuild += async (SocketGuild guild) => await JoinedGuild(guild);
            client.LeftGuild   += async (SocketGuild guild) => await LeftGuild(guild);

            // Here we can initialize the service that will register and execute our commands
            await services.GetRequiredService<CommandHandler>().InitializeAsync();

            await client.LoginAsync(TokenType.Bot, configuration["BotToken"]);
            await client.StartAsync();

            await client.SetGameAsync(name: "/magus invite", type: ActivityType.Playing);

            services.GetRequiredService<TIService>().Initialise();

            await host.StartAsync(); // Start now, after initial scheduled tasks have had a chance to be registered, for RunOnceAtStart
            await Task.Delay(Timeout.Infinite);
        }

        static async Task RegisterModules()
        {
            _logger.LogInformation("Registering Modules");
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
            //foreach (var guild in configuration.GetSection("ManagementGuilds").Get<ulong[]>())
            //    await _interactionService.AddModulesToGuildAsync(guild, true, modules: modules.Where(x => x.Value == Location.MANAGEMENT).Select(x => x.Key).ToArray());

            _logger.LogInformation("Complete Module Registration");
        }

        static async Task JoinedGuild(SocketGuild guild)
        {
            _logger.LogInformation("Added to guild Name: {0} ID: {1} Members: {3}, at {4}", guild.Name, guild.Id, guild.MemberCount, guild.CurrentUser.JoinedAt);
        }

        static async Task LeftGuild(SocketGuild guild)
        {
            _logger.LogInformation("Removed from guild Name: {0} ID: {1} Members: {3}, at {4}", guild.Name, guild.Id, guild.MemberCount, DateTimeOffset.UtcNow);
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

        static IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
            => serviceCollection
                .AddLogging(x => x.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger()))
                .AddSingleton(configuration)
                .AddSingleton(x => new Configuration(configuration))
                .AddSingleton<IAsyncDataService, MongoDBService>()
                .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.AllUnprivileged }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
                .AddSingleton(x => new HttpClient())
                //.AddHttpClient()
                .AddSingleton<Services.IWebhook>(x => new DiscordWebhook())
                //.AddTransient<DotaUpdater>()
                //.AddTransient<PatchListUpdater>()
                //.AddTransient<EntityUpdater>()
                //.AddTransient<PatchNoteUpdater>()
                .AddScheduler()
                .AddSingleton<TIService>();

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