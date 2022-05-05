using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Magus.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Magus.Bot
{
    class Bot
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "MAGUS_")
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets<Bot>()
                .Build();

        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }

        static async Task RunAsync()
        {
            using var services = ConfigureServices();

            var client = services.GetRequiredService<DiscordSocketClient>();
            var commands = services.GetRequiredService<InteractionService>();

            client.Log += LogAsync;
            commands.Log += LogAsync;

            // Slash Commands and Context Commands are can be automatically registered, but this process needs to happen after the client enters the READY state.
            // Since Global Commands take around 1 hour to register, we should use a test guild to instantly update and test our commands. To determine the method we should
            // register the commands with, we can check whether we are in a DEBUG environment and if we are, we can register the commands to a predetermined test guild.
            client.Ready += async () =>
            {
                if (IsDebug())
                    await commands.RegisterCommandsToGuildAsync(configuration.GetValue<ulong>("testGuild"), true);
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
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        static ServiceProvider ConfigureServices()
            => new ServiceCollection()
                .AddSingleton(configuration)
                .AddSingleton<IDatabaseService>(x => new LiteDBService(configuration.GetSection("DatabaseService")))
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
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