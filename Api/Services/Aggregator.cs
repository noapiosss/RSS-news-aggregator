using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Api.Helpers.Interfaces;
using Api.Services.Interfaces;
using Contracts.Database;
using Domain.Queries;
using Domain.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Api.Services
{
    public class Aggregator : IAggregator
    {
        private readonly ISyndicationConverter _syndicationConverter;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<Aggregator> _logger;

        public Aggregator(ISyndicationConverter syndicationConverter, IServiceScopeFactory serviceScopeFactory, ILogger<Aggregator> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _syndicationConverter = syndicationConverter;
            _logger = logger;
        }

        public async Task AggregateAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            GetAllFeedsQuery getAllFeedsQuery = new() { };
            GetAllFeedsQueryResult getAllFeedsQueryResult = await mediator.Send(getAllFeedsQuery, cancellationToken);

            _logger.LogInformation("Updating feeds started")

            foreach (Feed feed in getAllFeedsQueryResult.Feeds)
            {
                if (feed.SkipDays.Contains(DateTime.Now.DayOfWeek.ToString()) || feed.SkipDays.Contains(DateTime.Now.Hour.ToString()))
                {
                    continue;
                }

                using XmlReader reader = XmlReader.Create(feed.Link);
                SyndicationFeed syndicationFeed = SyndicationFeed.Load(reader);

                List<Post> newPosts = new();
                foreach (SyndicationItem syndicationItem in syndicationFeed.Items.Where(i => i.PublishDate.UtcDateTime > feed.LastUpdate))
                {
                    newPosts.Add(_syndicationConverter.SyndicationItemToPost(syndicationItem, feed.Id));
                }

                UpdateFeedPostsCommand updateFeedPostsCommand = new()
                {
                    FeedId = feed.Id,
                    Posts = newPosts.OrderBy(p => p.PubDate).ToList()
                };
                _ = await mediator.Send(updateFeedPostsCommand, cancellationToken);
            }

            _logger.LogInformation("Updating feeds ended")
        }

        public async Task DeleteSubscriblessFeed(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            _logger.LogInformation("Removing subscribless feeds started")

            GetSubscriblessFeedsQuery getSubscriblessFeedsQuery = new();
            GetSubscriblessFeedsQueryResult getSubscriblessFeedsQueryResult = await mediator.Send(getSubscriblessFeedsQuery, cancellationToken);
            foreach (Feed feed in getSubscriblessFeedsQueryResult.Feeds)
            {
                DeleteFeedCommand deleteFeedCommand = new()
                {
                    FeedId = feed.Id
                };
                _ = await mediator.Send(deleteFeedCommand, cancellationToken);
            }

            _logger.LogInformation("Removing subscribless feeds ended")
        }
    }
}