using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;

namespace Domain.Commands
{
    public class SubscribeToFeedCommand : IRequest<SubscribeToFeedCommandResult>
    {
        public string Username { get; set; }
        public Feed Feed { get; set; }
    }

    public class SubscribeToFeedCommandResult
    {
        public Feed Feed { get; set; }
    }

    internal class SubscribeToFeedCommandHandler : IRequestHandler<SubscribeToFeedCommand, SubscribeToFeedCommandResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public SubscribeToFeedCommandHandler(RSSNewsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SubscribeToFeedCommandResult> Handle(SubscribeToFeedCommand request, CancellationToken cancellationToken)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            Feed feed = await _dbContext.Feeds.FirstOrDefaultAsync(f => f.Link == request.Feed.Link);

            //check if user exists
            if (user == null)
            {
                return new()
                {
                    Feed = null
                };
            }

            //check if feed exists
            if (feed == null)
            {
                feed = request.Feed;
                _ = await _dbContext.AddAsync(feed, cancellationToken);
                _ = await _dbContext.SaveChangesAsync(cancellationToken);
            }

            if (await _dbContext.Subscriptions.AnyAsync(s => s.UserId == user.Id && s.FeedId == feed.Id))
            {
                return new()
                {
                    Feed = null
                };
            }

            Subscription subscription = new()
            {
                UserId = user.Id,
                FeedId = feed.Id
            };

            _ = await _dbContext.AddAsync(subscription, cancellationToken);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new()
            {
                Feed = feed
            };
        }
    }
}