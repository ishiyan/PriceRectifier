using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PriceRectifier.Alignment;
using PriceRectifier.ColoredConsoleLogger;
using PriceRectifier.Options;
using PriceRectifier.Topline;

namespace PriceRectifier
{
    internal class Program
    {
        private static async Task Main(string[] args) =>
            await CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                })
                .ConfigureLogging((hostingContext, logging) => {
                    logging.ClearProviders();
                    // logging.AddConsole();
                    logging.AddColoredConsoleLogger();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IToplineInputOptions, ToplineInputOptions>();
                    services.AddSingleton<IDataAlignmentOptions, DataAlignmentOptions>();
                    services.AddSingleton<IDataConsistencyOptions, DataConsistencyOptions>();
                    services.AddSingleton<IOutputOptions, OutputOptions>();
                    services.AddSingleton<IAlignedTimeRange, AlignedTimeRange>();

                    services.AddScoped<IToplineRepository, ToplineRepository>();

                    services.AddHostedService<Worker>();
                })
                .UseConsoleLifetime();
    }
}
