using Coravel;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GraphQL;
using Magus.Bot.Services;
using Magus.Common.Options;
using Magus.Data;
using Magus.Data.Enums;
using Magus.Data.Extensions;
using Magus.Data.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Prometheus;
using Prometheus.SystemMetrics;
using Serilog;

namespace Magus.Bot;

class Bot
{
    private static readonly GatewayIntents GATEWAY_INTENTS = GatewayIntents.Guilds;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private static ILogger<Bot> _logger;
    private static IAsyncDataService _db;
    private static IServiceProvider _services;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    static void Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(x => x.AddEnvironmentVariables(prefix: "MAGUS_"))
            .ConfigureServices((context, serviceCollection) => ConfigureServices(context.Configuration, serviceCollection))
            .UseSerilog((hostingContext, services, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration))
            .Build();

        _services = host.Services;
        _logger = _services.GetRequiredService<ILogger<Bot>>();
        _db = _services.GetRequiredService<IAsyncDataService>();
        RunAsync(host).GetAwaiter().GetResult();
    }

    static async Task RunAsync(IHost host)
    {

        var botSettings        = _services.GetRequiredService<IOptions<BotSettings>>().Value;
        var client             = _services.GetRequiredService<DiscordSocketClient>();
        var interactionService = _services.GetRequiredService<InteractionService>();
        var interactionHandler = _services.GetRequiredService<InteractionHandler>();

        using var metricsServer = new MetricServer(hostname: botSettings.MetricHost, port: botSettings.MetricPort);
        metricsServer.Start();

        client.Log += LogDiscord;
        interactionService.Log += LogDiscord;

        client.JoinedGuild += JoinedGuild;
        client.LeftGuild += LeftGuild;

        await interactionHandler.InitialiseAsync();
        //await services.GetRequiredService<TIService>().Initialise();
        await _services.GetRequiredService<DPCService>().InitialiseAsync();
        await _services.GetRequiredService<LocalisationService>().InitialiseAsync();
        await _services.GetRequiredService<AnnouncementService>().InitialiseAsync();

        if (IsDebug())
            client.Ready += async () => await interactionService.RegisterCommandsToGuildAsync(botSettings.DevGuild, true);
        else
            client.Ready += interactionHandler.RegisterModulesAsync;

        // Set the Guild Gauge to current guilds on ready, and inc/dec in respective event.
        // This could be moved to a separate scheduled task to ensure correct (such as missed gateway events), but not critical.
        client.Ready += async () => await Task.Run(() => MagusMetrics.Guilds.Set(client.Guilds.Count));

        await client.LoginAsync(TokenType.Bot, botSettings.BotToken);
        await client.StartAsync();
        await client.SetGameAsync(name: botSettings.Status.Title, type: (ActivityType)botSettings.Status.Type);

        await host.RunAsync();
    }

    static async Task JoinedGuild(SocketGuild guild)
    {
        _logger.LogInformation("Added to guild Name: {name} ID: {id} Members: {count}, at {joined}", guild.Name, guild.Id, guild.MemberCount, guild.CurrentUser.JoinedAt);
        await _db.UpsertGuildRecord(guild, DiscordGuildAction.Joined);
        MagusMetrics.Guilds.Inc();
    }

    static async Task LeftGuild(SocketGuild guild)
    {
        _logger.LogInformation("Removed from guild Name: {name} ID: {id} Members: {count}, at {joined}", guild.Name, guild.Id, guild.MemberCount, DateTimeOffset.UtcNow);
        await _db.UpsertGuildRecord(guild, DiscordGuildAction.Left);
        MagusMetrics.Guilds.Dec();
    }

    static Task LogDiscord(LogMessage message)
    {
        // Any logs to ignore, check here first
        switch (message.Exception)
        {
            case GatewayReconnectException: // Logs already show a disconnect and reconnect. Other errors should show.
                return Task.CompletedTask;
        }

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
            .Configure<LocalisationOptions>(settings => config.GetSection("Localisation").Bind(settings))
            .AddSystemMetrics()
            .AddScheduler()
            .AddHttpClient()
            .AddSingleton<IAsyncDataService, MongoDBService>()
            .AddSingleton<MeilisearchService>()
            .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GATEWAY_INTENTS }))
            .AddSingleton(x => new InteractionServiceConfig() { InteractionCustomIdDelimiters = [Constants.CustomIdGroupDelimiter], UseCompiledLambda = true })
            .AddSingleton<InteractionService>()
            .AddSingleton<InteractionHandler>()
            .AddSingleton<LocalisationService>()
            .AddSingleton<StratzService>()
            .AddSingleton<DPCService>()
            .AddSingleton<TIService>()
            .AddSingleton<AnnouncementService>();

    public static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}
