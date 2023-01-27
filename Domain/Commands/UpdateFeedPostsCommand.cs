using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Domain.Commands
{
    public class UpdateFeedPostsCommand : IRequest<UpdateFeedPostsCommandResult>
    {
        public Feed Feed { get; set; }
        public ICollection<Post> Posts { get; set; }
    }

    public class UpdateFeedPostsCommandResult
    {
        public Feed Feed { get; set; }
    }

    internal class UpdateFeedPostsCommandHandler : IRequestHandler<UpdateFeedPostsCommand, UpdateFeedPostsCommandResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public UpdateFeedPostsCommandHandler(RSSNewsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UpdateFeedPostsCommandResult> Handle(UpdateFeedPostsCommand request, CancellationToken cancellationToken)
        {
            if (request.Posts.Count == 0)
            {
                return new()
                {
                    Feed = request.Feed
                };
            }

            foreach (Post newPost in request.Posts)
            {
                newPost.FeedId = request.Feed.Id;
                _ = await _dbContext.AddAsync(newPost, cancellationToken);
            }
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            Feed feed = await _dbContext.Feeds.FirstOrDefaultAsync(f => f.Id == request.Feed.Id, cancellationToken);
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