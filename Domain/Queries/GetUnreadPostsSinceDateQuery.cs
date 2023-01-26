using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;

namespace domain.Queries
{
    public class GetUnreadPostsSinceDateQuery : IRequest<GetUnreadPostsSinceDateQueryResult>
    {
        public string Username { get; set; }
        public DateTime SinceDate { get; set; }
    }

    public class GetUnreadPostsSinceDateQueryResult
    {
        public ICollection<Post> Posts { get; set; }
    }

    internal class GetUnreadPostsSinceDateQueryHandler : IRequestHandler<GetUnreadPostsSinceDateQuery, GetUnreadPostsSinceDateQueryResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public GetUnreadPostsSinceDateQueryHandler(RSSNewsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetUnreadPostsSinceDateQueryResult> Handle(GetUnreadPostsSinceDateQuery request, CancellationToken cancellationToken)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

            if (user == null)
            {
                return new()
                {
                    Posts = new List<Post>()
                };
            }

            IQueryable<Post> allPosts = _dbContext.Subscriptions
                .Where(s => s.UserId == user.Id)
                .Include(s => s.Feed)
                .Select(s => s.Feed)
                .Include(f => f.Posts)
                .Select(f => f.Posts)
                .Cast<Post>()
                .Where(p => p.PubDate > request.SinceDate);

            IQueryable<Post> readPosts = _dbContext.ReadPosts
                .Where(rp => rp.UserId == user.Id)
                .Include(rp => rp.Post)
                .Select(rp => rp.Post)
                .Where(rp => rp.PubDate > request.SinceDate);

            List<Post> unreadPosts = await allPosts.Except(readPosts).ToListAsync(cancellationToken);

            return new()
            {
                Posts = unreadPosts
            };
        }
    }
}