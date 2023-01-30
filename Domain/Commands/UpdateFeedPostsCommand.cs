using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Domain.Base;
using Microsoft.Extensions.Logging;
using Domain.Excetions;
using Contracts.Http;

namespace Domain.Commands
{
    public class UpdateFeedPostsCommand : IRequest<UpdateFeedPostsCommandResult>
    {
        public int FeedId { get; set; }
        public ICollection<Post> Posts { get; set; }
    }

    public class UpdateFeedPostsCommandResult
    {
        public Feed Feed { get; set; }
    }

    internal class UpdateFeedPostsCommandHandler : BaseHandler<UpdateFeedPostsCommand, UpdateFeedPostsCommandResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public UpdateFeedPostsCommandHandler(RSSNewsDbContext dbContext, ILogger<UpdateFeedPostsCommandHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
        }

        protected override async Task<UpdateFeedPostsCommandResult> HandleInternal(UpdateFeedPostsCommand request, CancellationToken cancellationToken)
        {
            Feed feed = await _dbContext.Feeds.FirstOrDefaultAsync(f => f.Id == request.FeedId, cancellationToken);

            if (feed == null)
            {
                throw new RSSNewsReaderException(ErrorCode.FeedNotFound, $"Feed with id '{request.FeedId}' not found");
            }

            if (request.Posts.Count == 0)
            {
                return new()
                {
                    Feed = feed
                };
            }

            foreach (Post post in request.Posts)
            {
                _ = await _dbContext.AddAsync(post, cancellationToken);
            }
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            feed.LastUpdate = request.Posts.MaxBy(p => p.PubDate).PubDate;
            _ = _dbContext.Feeds.Update(feed);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new()
            {
                Feed = feed
            };
        }
    }
}