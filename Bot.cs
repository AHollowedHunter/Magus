using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
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

        private static ILogger<Bot> _logger;

        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }

        static async Task RunAsync()
        {
            using var services = ConfigureServices();
            _logger = services.GetRequiredService<ILogger<Bot>>();

            var client = services.GetRequiredService<DiscordSocketClient>();
            var commands = services.GetRequiredService<InteractionService>();


            client.Log += LogAsync;
            commands.Log += LogAsync;

            client.Ready += async () =>
            {
                if (IsDebug())
                    await commands.RegisterCommandsToGuildAsync(configuration.GetValue<ulong>("TestGuild"), true);
                else
                    await commands.RegisterCommandsGloballyAsync(true);
            };

            // Here we can initialize the service that will register and execute our commands
            await services.GetRequiredService<CommandHandler>().InitializeAsync();

            await client.LoginAsync(TokenType.Bot, configuration["BotToken"]);
            await client.StartAsync();

            await client.SetGameAsync(name: "\"/magus help\"", type: ActivityType.Playing);

            await Task.Delay(Timeout.Infinite);
        }

        static Task LogAsync(LogMessage message)
        {
            var severity = message.Severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Debug,
                LogSeverity.Debug => LogLevel.Trace,
                _ => LogLevel.Information
            };
            _logger.Log(severity, message.Exception, $"[{message.Source}] {message.Message ?? message.Exception.Message}");
            return Task.CompletedTask;
        }

        static ServiceProvider ConfigureServices()
            => new ServiceCollection()
                .AddLogging(x => x.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger()))
                .AddSingleton(configuration)
                .AddSingleton<IDatabaseService>(x => new LiteDBService(configuration.GetSection("DatabaseService")))
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.AllUnprivileged }))
                .AddSingleton<CommandService>()
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