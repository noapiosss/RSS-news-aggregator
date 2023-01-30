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
    public class UnsubscribeFromFeedCommand : IRequest<UnsubscribeFromFeedCommandResult>
    {
        public string Username { get; set; }
        public int FeedId { get; set; }
    }

    public class UnsubscribeFromFeedCommandResult
    {
        public bool UnsubscribeIsSuccessful { get; set; }
    }

    internal class UnsubscribeFromFeedCommandHandler : BaseHandler<UnsubscribeFromFeedCommand, UnsubscribeFromFeedCommandResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public UnsubscribeFromFeedCommandHandler(RSSNewsDbContext dbContext, ILogger<UnsubscribeFromFeedCommandHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
        }

        protected override async Task<UnsubscribeFromFeedCommandResult> HandleInternal(UnsubscribeFromFeedCommand request, CancellationToken cancellationToken)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
            Feed feed = await _dbContext.Feeds.FirstOrDefaultAsync(f => f.Id == request.FeedId, cancellationToken: cancellationToken);

            if (user == null)
            {
                throw new RSSNewsReaderException(ErrorCode.UserNotFound, $"User '{request.Username}' not found");
            }

            if (feed == null)
            {
                throw new RSSNewsReaderException(ErrorCode.FeedNotFound, $"Feed with id '{request.FeedId}' not found");
            }

            if (!await _dbContext.Subscriptions.AnyAsync(s => s.UserId == user.Id && s.FeedId == feed.Id, cancellationToken: cancellationToken))
            {
                throw new RSSNewsReaderException(ErrorCode.SubscriptionNotFound, $"User '{request.Username}' is not subscribed to feed with id '{request.FeedId}'");
            }

            Subscription subscription = new()
            {
                UserId = user.Id,
                FeedId = request.FeedId
            };

            _ = _dbContext.Subscriptions.Attach(subscription);
            _ = _dbContext.Subscriptions.Remove(subscription);
            _ = _dbContext.SaveChanges();

            return new()
            {
                UnsubscribeIsSuccessful = true
            };
        }
    }
}