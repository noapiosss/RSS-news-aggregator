using System.ServiceModel.Syndication;
using Contracts.Database;

namespace Api.Helpers.Interfaces
{
    public interface ISyndicationConverter
    {
        public Feed SyndicationFeedToFeed(SyndicationFeed syndicationFeed, string requestLink);
        public Post SyndicationItemToPost(SyndicationItem syndicationItem, int feedId);
    }
}