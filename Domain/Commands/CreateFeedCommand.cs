using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;
using Domain.Base;
using Microsoft.Extensions.Logging;

namespace Domain.Commands
{
    public class CreateFeedCommand : IRequest<CreateFeedCommandResult>
    {
        public Feed Feed { get; set; }
    }

    public class CreateFeedCommandResult
    {
        public Feed Feed { get; set; }
    }

    internal class CreateFeedCommandHandler : BaseHandler<CreateFeedCommand, CreateFeedCommandResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public CreateFeedCommandHandler(RSSNewsDbContext dbContext, ILogger<CreateFeedCommandHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
        }

        protected override async Task<CreateFeedCommandResult> HandleInternal(CreateFeedCommand request, CancellationToken cancellationToken)
        {
            Feed feed = await _dbContext.Feeds.FirstOrDefaultAsync(f => f.Link == request.Feed.Link, cancellationToken: cancellationToken);

            if (feed != null)
            {
                return new()
                {
                    Feed = feed
                };
            }

            feed = request.Feed;

            _ = await _dbContext.AddAsync(feed, cancellationToken);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new()
            {
                Feed = feed
            };
        }
    }
}