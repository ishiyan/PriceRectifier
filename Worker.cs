using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PriceRectifier.Alignment;
using PriceRectifier.Topline;

namespace PriceRectifier
{
    internal class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IHostApplicationLifetime appLifetime;
        private readonly IToplineRepository toplineRepository;
        private readonly IAlignedTimeRange alignedTimeRange;

        public Worker(ILogger<Worker> logger, IHostApplicationLifetime appLifetime, IToplineRepository toplineRepository,
            IAlignedTimeRange alignedTimeRange)
        {
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.toplineRepository = toplineRepository;
            this.alignedTimeRange = alignedTimeRange;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                logger.LogInformation($"Starting execution.");
                await toplineRepository.ImportToplineInstruments(stoppingToken);

                await toplineRepository.ValidateData(stoppingToken);

                await alignedTimeRange.Align(stoppingToken);

                await toplineRepository.ExportData(stoppingToken);
                logger.LogInformation($"Exported {ToplineRepository.ToplineInstruments.Count} topline instruments.");

                logger.LogTrace("this is a trace");
                logger.LogDebug("this is debug");
                logger.LogInformation("this is an info");
                logger.LogWarning("this is a warning");
                logger.LogError("this is an error");
                logger.LogCritical("this is a critical");
            }
            catch (Exception ex)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("Cancellation requested, breaking iterations.");
                }
                else
                {
                    logger.LogCritical(ex, "Unhandled exception caught, panic.");
                }
            }

            appLifetime.StopApplication();
        }
    }
}
