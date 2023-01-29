using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using Api.Helpers.Interfaces;
using Contracts.Database;

namespace Api.Helpers
{
    public class SyndicationConverter : ISyndicationConverter
    {
        public Feed SyndicationFeedToFeed(SyndicationFeed syndicationFeed, string requestLink)
        {
            Feed feed = new()
            {
                Title = syndicationFeed.Title.Text,
                Description = syndicationFeed.Description.Text,
                Link = requestLink,
                Author = GetAuthors(syndicationFeed.Authors),
                Language = syndicationFeed.Language,
                Copyright = syndicationFeed.Copyright?.Text,
                Category = string.Join(" ", syndicationFeed.Categories),
                Generator = syndicationFeed.Generator,
                Docs = syndicationFeed.Documentation?.Uri.ToString(),
                TTL = syndicationFeed.TimeToLive?.ToString(),
                Image = syndicationFeed.ImageUrl?.ToString(),
                TextInputTitle = syndicationFeed.TextInput?.Title,
                TextInputDescription = syndicationFeed.TextInput?.Description,
                TextInputName = syndicationFeed.TextInput?.Name,
                TextInputLink = syndicationFeed.TextInput?.Link.ToString(),
                SkipHours = string.Join(" ", syndicationFeed.SkipHours),
                SkipDays = string.Join(" ", syndicationFeed.SkipDays),
                LastUpdate = new()
            };

            return feed;
        }

        public Post SyndicationItemToPost(SyndicationItem syndicationItem, int feedId)
        {
            Post post = new()
            {
                Title = syndicationItem.Title.Text,
                Description = syndicationItem.Summary.Text,
                PubDate = syndicationItem.PublishDate.UtcDateTime,
                Category = string.Join(" ", syndicationItem.Categories),
                GUID = syndicationItem.Id,
                Source = syndicationItem.SourceFeed?.Links.FirstOrDefault().Uri.ToString(),
                Link = syndicationItem.Links.FirstOrDefault().Uri.ToString(),
                FeedId = feedId
            };

            return post;
        }

        private static string GetAuthors(Collection<SyndicationPerson> authors)
        {
            StringBuilder sb = new();
            foreach (SyndicationPerson author in authors)
            {
                _ = sb.Append($"{author.Name} ({author.Email})");
            }

            return string.Join(", ", sb);
        }
    }
}