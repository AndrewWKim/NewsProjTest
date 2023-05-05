using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AvaTradeNews.Services
{
    public interface INewsGettingService
	{
        Task GetNewsFromSource();
	}

	public class PolygonNewsService : INewsGettingService 
    {
        public async Task GetNewsFromSource()
		{
            /* Get HTTP client
               Make Get Request
               Map response to db entity
               Save data in database
            */
		}
    }


	public class NewsBackgroundService : BackgroundService
	{
        private readonly ILogger<NewsBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1);
        private readonly IServiceScopeFactory _factory;

        // We can get the period from config file for example
        public NewsBackgroundService(IServiceProvider services,
            ILogger<NewsBackgroundService> logger)
        {
            Services = services;
            _logger = logger;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("News Getting Service is running.");

            // PeriodicTimer was released in .NET 6
            using PeriodicTimer timer = new PeriodicTimer(_period);
            while (
                !stoppingToken.IsCancellationRequested &&
                await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    GetNews();
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Failed to execute NewsBackgroundService with exception: {ex.Message}.");
                }
            }
        }

        private async Task GetNews()
        {
            _logger.LogInformation("");

            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<INewsGettingService>();

                await scopedProcessingService.GetNewsFromSource();
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("News Getting Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}

/*
 * For example in Progrma cs register services:

builder.Services.AddScoped<INewsGettingService, PolygonNewsService>();
builder.Services.AddSingleton<NewsBackgroundService>();
builder.Services.AddHostedService(
    provider => provider.GetRequiredService<NewsBackgroundService>());
 */
