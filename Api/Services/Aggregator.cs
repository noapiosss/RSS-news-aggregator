using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Api.Helpers.Interfaces;
using Api.Services.Interfaces;
using Contracts.Database;
using domain.Queries;
using Domain.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Services
{
    public class Aggregator : IAggregator
    {
        //private readonly IMediator _mediator;
        private readonly ISyndicationConverter _syndicationConverter;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public Aggregator(ISyndicationConverter syndicationConverter, IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _syndicationConverter = syndicationConverter;
        }

        public async Task AggregateAsync()
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            GetAllFeedsQuery getAllFeedsQuery = new() { };
            GetAllFeedsQueryResult getAllFeedsQueryResult = await mediator.Send(getAllFeedsQuery);

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
                    Feed = feed,
                    Posts = newPosts.OrderBy(p => p.PubDate).ToList()
                };
                _ = await mediator.Send(updateFeedPostsCommand);
            }
        }
    }
}