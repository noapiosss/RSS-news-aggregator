using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;
using Domain.Base;
using Microsoft.Extensions.Logging;
using Domain.Excetions;
using Contracts.Http;

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

    internal class SubscribeToFeedCommandHandler : BaseHandler<SubscribeToFeedCommand, SubscribeToFeedCommandResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public SubscribeToFeedCommandHandler(RSSNewsDbContext dbContext, ILogger<SubscribeToFeedCommandHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
        }

        protected override async Task<SubscribeToFeedCommandResult> HandleInternal(SubscribeToFeedCommand request, CancellationToken cancellationToken)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken: cancellationToken);
            Feed feed = await _dbContext.Feeds.FirstOrDefaultAsync(f => f.Id == request.Feed.Id, cancellationToken: cancellationToken);

            if (user == null)
            {
                throw new RSSNewsReaderException(ErrorCode.UserNotFound, $"User '{request.Username}' not found");
            }

            if (feed == null)
            {
                throw new RSSNewsReaderException(ErrorCode.FeedNotFound, $"Feed with id '{request.Feed.Id}' not found");
            }

            if (await _dbContext.Subscriptions.AnyAsync(s => s.UserId == user.Id && s.FeedId == feed.Id, cancellationToken: cancellationToken))
            {
                return new()
                {
                    Feed = feed
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