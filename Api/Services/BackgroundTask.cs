using System;
using System.Threading;
using System.Threading.Tasks;
using Api.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Api.Services
{
    public class BackgroundAggregation : BackgroundService
    {
        private readonly IAggregator _aggregator;

        public BackgroundAggregation(IAggregator aggregator)
        {
            _aggregator = aggregator;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _aggregator.AggregateAsync();
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // rework to set correct time (at the beginning of every hour)
            }
        }
    }
}