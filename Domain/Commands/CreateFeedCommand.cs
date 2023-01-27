using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;

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

    internal class CreateFeedCommandHandler : IRequestHandler<CreateFeedCommand, CreateFeedCommandResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public CreateFeedCommandHandler(RSSNewsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CreateFeedCommandResult> Handle(CreateFeedCommand request, CancellationToken cancellationToken)
        {
            Feed feed = await _dbContext.Feeds.FirstOrDefaultAsync(f => f.Link == request.Feed.Link, cancellationToken: cancellationToken);

            //check if user exists
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