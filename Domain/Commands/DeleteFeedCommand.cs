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
    public class DeleteFeedCommand : IRequest<DeleteFeedCommandResult>
    {
        public int FeedId { get; set; }
    }

    public class DeleteFeedCommandResult
    {
        public bool IsDeleted { get; set; }
    }

    internal class DeleteFeedCommandHandler : BaseHandler<DeleteFeedCommand, DeleteFeedCommandResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public DeleteFeedCommandHandler(RSSNewsDbContext dbContext, ILogger<DeleteFeedCommandHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
        }

        protected override async Task<DeleteFeedCommandResult> HandleInternal(DeleteFeedCommand request, CancellationToken cancellationToken)
        {
            Feed feed = await _dbContext.Feeds.FirstOrDefaultAsync(f => f.Id == request.FeedId, cancellationToken: cancellationToken);

            if (feed == null)
            {
                throw new RSSNewsReaderException(ErrorCode.FeedNotFound, $"Feed with id '{request.FeedId}' not found");
            }

            _ = _dbContext.Feeds.Attach(feed);
            _ = _dbContext.Feeds.Remove(feed);
            _ = _dbContext.SaveChanges();

            return new()
            {
                IsDeleted = true
            };
        }
    }
}