using Magus.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        private static readonly IDatabaseService db      = services.GetRequiredService<IDatabaseService>();

        public static async Task Main()
        {

            var patchNoteUpdater = services.GetRequiredService<PatchNoteUpdater>();
            var entityUpdater = services.GetRequiredService<EntityUpdater>();

            var stopwatch = new Stopwatch();
            stopwatch.Start();


            var entityInfotask = entityUpdater.Update();
            await entityInfotask;
            //var patchNoteTask = patchNoteUpdater.Update();
            //await patchNoteTask;

            stopwatch.Stop();
            var timeTaken = stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine(string.Format("Total Time Taken: {0:0.#}s", timeTaken));
            db.Dispose();
        }

        static ServiceProvider ConfigureServices()
            => new ServiceCollection()
                .AddLogging(x => x.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger()))
                .AddSingleton(configuration)
                .AddSingleton<IDatabaseService>(x => new LiteDBService(configuration.GetSection("DatabaseService")))
                .AddSingleton(x => new HttpClient())
                .AddSingleton<PatchNoteUpdater>()
                .AddSingleton<EntityUpdater>()
                .BuildServiceProvider();
    }
}