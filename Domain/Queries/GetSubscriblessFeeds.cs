using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;
using Domain.Base;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Domain.Queries
{
    public class GetSubscriblessFeedsQuery : IRequest<GetSubscriblessFeedsQueryResult>
    {
    }

    public class GetSubscriblessFeedsQueryResult
    {
        public ICollection<Feed> Feeds { get; set; }
    }

    internal class GetSubscriblessFeedsQueryHandler : BaseHandler<GetSubscriblessFeedsQuery, GetSubscriblessFeedsQueryResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public GetSubscriblessFeedsQueryHandler(RSSNewsDbContext dbContext, ILogger<GetSubscriblessFeedsQuery> logger) : base(logger)
        {
            _dbContext = dbContext;
        }

        protected override async Task<GetSubscriblessFeedsQueryResult> HandleInternal(GetSubscriblessFeedsQuery request, CancellationToken cancellationToken)
        {
            List<Feed> feeds = await _dbContext.Feeds
                .Include(f => f.Subscribers)
                .Where(f => !f.Subscribers.Any())
                .ToListAsync(cancellationToken);

            return new()
            {
                Feeds = feeds
            };
        }
    }
}