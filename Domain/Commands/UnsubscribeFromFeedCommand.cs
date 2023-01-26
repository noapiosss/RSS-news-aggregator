using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;

namespace Domain.Commands
{
    public class UnsubscribeFromFeedCommand : IRequest<UnsubscribeFromFeedCommandResult>
    {
        public string Username { get; set; }
        public Feed Feed { get; set; }
    }

    public class UnsubscribeFromFeedCommandResult
    {
        public Feed Feed { get; set; }
    }

    internal class UnsubscribeFromFeedCommandHandler : IRequestHandler<UnsubscribeFromFeedCommand, UnsubscribeFromFeedCommandResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public UnsubscribeFromFeedCommandHandler(RSSNewsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UnsubscribeFromFeedCommandResult> Handle(UnsubscribeFromFeedCommand request, CancellationToken cancellationToken)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
            Feed feed = await _dbContext.Feeds.FirstOrDefaultAsync(f => f.Link == request.Feed.Link);

            if (user == null || feed == null)
            {
                return new()
                {
                    Feed = null
                };
            };

            Subscription subscription = new()
            {
                UserId = user.Id,
                FeedId = feed.Id
            };

            _ = _dbContext.Subscriptions.Attach(subscription);
            _ = _dbContext.Subscriptions.Remove(subscription);
            _ = _dbContext.SaveChanges();

            return new()
            {
                Feed = null
            };
        }
    }
}