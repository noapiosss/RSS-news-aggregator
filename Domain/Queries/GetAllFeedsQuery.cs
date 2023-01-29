using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;

namespace domain.Queries
{
    public class GetAllFeedsQuery : IRequest<GetAllFeedsQueryResult>
    {
    }

    public class GetAllFeedsQueryResult
    {
        public ICollection<Feed> Feeds { get; set; }
    }

    internal class GetAllFeedsQueryHandler : IRequestHandler<GetAllFeedsQuery, GetAllFeedsQueryResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public GetAllFeedsQueryHandler(RSSNewsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetAllFeedsQueryResult> Handle(GetAllFeedsQuery request, CancellationToken cancellationToken)
        {
            List<Feed> feeds = await _dbContext.Feeds.ToListAsync(cancellationToken);

            return new()
            {
                Feeds = feeds
            };
        }
    }
}