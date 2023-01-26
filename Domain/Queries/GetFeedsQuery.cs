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
    public class GetFeedsQuery : IRequest<GetFeedsQueryResult>
    {
        public string Username { get; set; }
    }

    public class GetFeedsQueryResult
    {
        public ICollection<Feed> Feeds { get; set; }
    }

    internal class GetFeedsQueryHandler : IRequestHandler<GetFeedsQuery, GetFeedsQueryResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public GetFeedsQueryHandler(RSSNewsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetFeedsQueryResult> Handle(GetFeedsQuery request, CancellationToken cancellationToken)
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