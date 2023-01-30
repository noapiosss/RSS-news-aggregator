using System;
using System.Threading;
using System.Threading.Tasks;
using Api.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Api.Services
{
    public class BackgroundAggregationService : BackgroundService
    {
        private readonly IAggregator _aggregator;

        public BackgroundAggregationService(IAggregator aggregator)
        {
            _aggregator = aggregator;
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