using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;
using System.Linq;
using Domain.Base;
using Microsoft.Extensions.Logging;
using Domain.Excetions;
using Contracts.Http;

namespace Domain.Queries
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

    internal class GetUnreadPostsSinceDateQueryHandler : BaseHandler<GetUnreadPostsSinceDateQuery, GetUnreadPostsSinceDateQueryResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public GetUnreadPostsSinceDateQueryHandler(RSSNewsDbContext dbContext, ILogger<GetUnreadPostsSinceDateQueryHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
        }

        protected override async Task<GetUnreadPostsSinceDateQueryResult> HandleInternal(GetUnreadPostsSinceDateQuery request, CancellationToken cancellationToken)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

            if (user == null)
            {
                throw new RSSNewsReaderException(ErrorCode.UserNotFound, $"User '{request.Username}' not found");
            }

            IQueryable<int> subscriptionIds = _dbContext.Subscriptions
                .Where(s => s.UserId == user.Id)
                .Include(s => s.Feed)
                .Select(s => s.Feed)
                .Select(f => f.Id);

            List<Post> subscriptionPosts = await _dbContext.Posts
                .Where(p => subscriptionIds.Contains(p.FeedId) && p.PubDate > request.SinceDate)
                .ToListAsync(cancellationToken);

            List<Post> readPosts = await _dbContext.ReadPosts
                .Where(rp => rp.UserId == user.Id)
                .Include(rp => rp.Post)
                .Select(rp => rp.Post)
                .Where(rp => rp.PubDate > request.SinceDate)
                .ToListAsync(cancellationToken);

            List<Post> unreadPosts = subscriptionPosts.Except(readPosts).ToList();

            return new()
            {
                Posts = unreadPosts
            };
        }
    }
}