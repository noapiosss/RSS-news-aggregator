using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;
using Domain.Base;
using Microsoft.Extensions.Logging;

namespace Domain.Queries
{
    public class GetAllFeedsQuery : IRequest<GetAllFeedsQueryResult>
    {
    }

    public class GetAllFeedsQueryResult
    {
        public ICollection<Feed> Feeds { get; set; }
    }

    internal class GetAllFeedsQueryHandler : BaseHandler<GetAllFeedsQuery, GetAllFeedsQueryResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public GetAllFeedsQueryHandler(RSSNewsDbContext dbContext, ILogger<GetAllFeedsQuery> logger) : base(logger)
        {
            _dbContext = dbContext;
        }

        protected override async Task<GetAllFeedsQueryResult> HandleInternal(GetAllFeedsQuery request, CancellationToken cancellationToken)
        {
            List<Feed> feeds = await _dbContext.Feeds.ToListAsync(cancellationToken);

            return new()
            {
                Feeds = feeds
            };
        }
    }
}