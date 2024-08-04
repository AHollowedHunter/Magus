﻿using Magus.Common.Options;
using Magus.Data;
using Magus.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using UltimyrArchives.Updater.Utils;

namespace UltimyrArchives.Updater;

public class Program
{
    public static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(AddConfiguration)
            .UseSerilog((hostingContext, _, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration))
            .ConfigureServices(ConfigureServices)
            .Build();

        await host.StartAsync();
        try
        {
            var dotaParser = host.Services.GetRequiredService<Updater>();
            await dotaParser.RunAsync();
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static void AddConfiguration(IConfigurationBuilder configurationBuilder)
        => configurationBuilder.AddEnvironmentVariables(prefix: "MAGUS_")
            .AddUserSecrets<Program>(optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

    static void ConfigureServices(HostBuilderContext context, IServiceCollection serviceCollection)
    {
        serviceCollection
            .Configure<UpdaterConfig>(settings => context.Configuration.GetSection("Updater").Bind(settings))
            .Configure<DataSettings>(settings => context.Configuration.GetSection("DataSettings").Bind(settings))
            .Configure<LocalisationOptions>(settings => context.Configuration.GetSection("Localisation").Bind(settings))
            .AddHttpClient()
            .AddSingleton<IAsyncDataService, MongoDBService>()
            .AddSingleton<MeilisearchService>()
            .AddTransient<GameFileProvider>()
            .AddSingleton<Updater>();
    }
}
