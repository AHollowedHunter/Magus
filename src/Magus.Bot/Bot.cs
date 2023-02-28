using Coravel;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Magus.Bot.Attributes;
using Magus.Bot.Services;
using Magus.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Magus.Bot
{
    class Bot
    {
        private static readonly GatewayIntents GATEWAY_INTENTS = GatewayIntents.Guilds;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private static ILogger<Bot> _logger;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        static void Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(x => x.AddEnvironmentVariables(prefix: "MAGUS_"))
                .ConfigureServices((context, serviceCollection) => ConfigureServices(context.Configuration, serviceCollection))
                .UseSerilog((hostingContext, services, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration))
                .Build();

            _logger = host.Services.GetRequiredService<ILogger<Bot>>();
            RunAsync(host).GetAwaiter().GetResult();
        }

        static async Task RunAsync(IHost host)
        {
            var services           = host.Services;
            var botSettings        = services.GetRequiredService<IOptions<BotSettings>>().Value;
            var client             = services.GetRequiredService<DiscordSocketClient>();
            var interactionService = services.GetRequiredService<InteractionService>();

            client.Log             += LogDiscord;
            interactionService.Log += LogDiscord;

            if (IsDebug())
                client.Ready += async () => await interactionService.RegisterCommandsToGuildAsync(botSettings.DevGuild, true);
            else
                client.Ready += async () => await RegisterModules(interactionService);

            client.JoinedGuild += async (SocketGuild guild) => await JoinedGuild(guild);
            client.LeftGuild   += async (SocketGuild guild) => await LeftGuild(guild);

            await services.GetRequiredService<CommandHandler>().InitializeAsync();
            await services.GetRequiredService<TIService>().Initialise();

            await client.LoginAsync(TokenType.Bot, botSettings.BotToken);
            await client.StartAsync();
            await client.SetGameAsync(name: botSettings.Status.Title, type: (ActivityType)botSettings.Status.Type);

            await host.RunAsync();
        }

        static async Task RegisterModules(InteractionService interactionService)
        {
            _logger.LogInformation("Registering Modules");
            var modules = new Dictionary<ModuleInfo, Location>();
            foreach (var module in interactionService.Modules)
                modules.Add(module, ((ModuleRegistration)module.Attributes.First(x => typeof(ModuleRegistration).IsAssignableFrom(x.GetType()))).Location);

            // Register GLOBAL commands
            await interactionService.AddModulesGloballyAsync(true, modules: modules.Where(x => x.Value == Location.GLOBAL).Select(x => x.Key).ToArray());

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

        static Task JoinedGuild(SocketGuild guild)
        {
            _logger.LogInformation("Added to guild Name: {name} ID: {id} Members: {count}, at {joined}", guild.Name, guild.Id, guild.MemberCount, guild.CurrentUser.JoinedAt);
            return Task.CompletedTask;
        }

        static Task LeftGuild(SocketGuild guild)
        {
            _logger.LogInformation("Removed from guild Name: {name} ID: {id} Members: {count}, at {joined}", guild.Name, guild.Id, guild.MemberCount, DateTimeOffset.UtcNow);
            return Task.CompletedTask;
        }

        static Task LogDiscord(LogMessage message)
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
            _logger.Log(severity, message.Exception, "[{source}] {message}", message.Source, message.Message ?? message.Exception.Message);
            return Task.CompletedTask;
        }

        static IServiceCollection ConfigureServices(IConfiguration config, IServiceCollection serviceCollection)
            => serviceCollection
                .Configure<BotSettings>(settings => config.GetSection("BotSettings").Bind(settings))
                .Configure<DataSettings>(settings => config.GetSection("DataSettings").Bind(settings))
                .AddScheduler()
                .AddSingleton<IAsyncDataService, MongoDBService>()
                .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GATEWAY_INTENTS }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
                .AddSingleton(x => new HttpClient())
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