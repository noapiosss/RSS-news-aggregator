using System;
using System.Threading;
using System.Threading.Tasks;
using Api.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.Services
{
    public class BackgroundAggregationService : BackgroundService
    {
        private readonly IAggregator _aggregator;
        private readonly ILogger<BackgroundAggregationService> _logger;

        public BackgroundAggregationService(IAggregator aggregator, ILogger<BackgroundAggregationService> logger)
        {
            _aggregator = aggregator;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime nextUpdateTime = DateTime.Now.AddHours(1);
                nextUpdateTime = nextUpdateTime.AddMinutes(-nextUpdateTime.Minute + 1);

                await Task.Delay(nextUpdateTime - DateTime.Now, stoppingToken);
                await _aggregator.DeleteSubscriblessFeed(stoppingToken);
                await _aggregator.AggregateAsync(stoppingToken);
            }
        }
    }
}