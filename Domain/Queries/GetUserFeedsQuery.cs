using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;
using System.Linq;

namespace domain.Queries
{
    public class GetUserFeedsQuery : IRequest<GetUserFeedsQueryResult>
    {
        public string Username { get; set; }
    }

    public class GetUserFeedsQueryResult
    {
        public ICollection<Feed> Feeds { get; set; }
    }

    internal class GetUserFeedsQueryHandler : IRequestHandler<GetUserFeedsQuery, GetUserFeedsQueryResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public GetUserFeedsQueryHandler(RSSNewsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetUserFeedsQueryResult> Handle(GetUserFeedsQuery request, CancellationToken cancellationToken)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

            if (user == null)
            {
                return new()
                {
                    Feeds = new List<Feed>()
                };
            }

            List<Feed> feeds = await _dbContext.Subscriptions
                .Where(s => s.UserId == user.Id)
                .Include(s => s.Feed)
                .Select(s => s.Feed)
                .ToListAsync(cancellationToken);

            return new()
            {
                Feeds = feeds
            };
        }
    }
}