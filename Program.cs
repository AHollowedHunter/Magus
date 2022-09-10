﻿using Magus.Common;
using Magus.Data;
using Serilog;
using System.Diagnostics;

namespace Magus.DataBuilder
{
    class Program
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables(prefix: "MAGUS_")
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<Program>()
            .Build();

        private static readonly ServiceProvider services = ConfigureServices();

        public static async Task Main()
        {
            var dotaUpdater = services.GetRequiredService<DotaUpdater>();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await dotaUpdater.Update(DotaUpdater.DotaInfo.ALL);

            stopwatch.Stop();
            var timeTaken = stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine(string.Format("Total Time Taken: {0:0.#}s", timeTaken));
        }

        static ServiceProvider ConfigureServices()
            => new ServiceCollection()
                .AddLogging(x => x.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger()))
                .AddSingleton(configuration)
                .AddSingleton(x => new Configuration(configuration))
                .AddSingleton<IAsyncDataService, MongoDBService>()
                .AddSingleton(x => new HttpClient())
                .AddSingleton<DotaUpdater>()
                .AddTransient<PatchNoteUpdater>()
                .AddTransient<EntityUpdater>()
                .AddTransient<PatchListUpdater>()
                .BuildServiceProvider();
    }
}