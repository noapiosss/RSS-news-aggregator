using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;
using System.Linq;
using Domain.Base;
using Domain.Excetions;
using Contracts.Http;
using Microsoft.Extensions.Logging;

namespace Domain.Queries
{
    public class GetUserFeedsQuery : IRequest<GetUserFeedsQueryResult>
    {
        public string Username { get; set; }
    }

    public class GetUserFeedsQueryResult
    {
        public ICollection<Feed> Feeds { get; set; }
    }

    internal class GetUserFeedsQueryHandler : BaseHandler<GetUserFeedsQuery, GetUserFeedsQueryResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public GetUserFeedsQueryHandler(RSSNewsDbContext dbContext, ILogger<GetUserFeedsQueryHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
        }

        protected override async Task<GetUserFeedsQueryResult> HandleInternal(GetUserFeedsQuery request, CancellationToken cancellationToken)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

            if (user == null)
            {
                throw new RSSNewsReaderException(ErrorCode.UserNotFound, $"User '{request.Username}' not found");
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