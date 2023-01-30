using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;
using Domain.Base;
using Microsoft.Extensions.Logging;
using Domain.Excetions;
using Contracts.Http;

namespace Domain.Commands
{
    public class MarkAsReadByIdCommand : IRequest<MarkAsReadByIdCommandResult>
    {
        public string Username { get; set; }
        public int PostId { get; set; }
    }

    public class MarkAsReadByIdCommandResult
    {
        public bool IsRead { get; set; }
    }

    internal class MarkAsReadByIdCommandHandler : BaseHandler<MarkAsReadByIdCommand, MarkAsReadByIdCommandResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public MarkAsReadByIdCommandHandler(RSSNewsDbContext dbContext, ILogger<MarkAsReadByIdCommandHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
        }

        protected override async Task<MarkAsReadByIdCommandResult> HandleInternal(MarkAsReadByIdCommand request, CancellationToken cancellationToken)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
            Post post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

            if (user == null)
            {
                throw new RSSNewsReaderException(ErrorCode.UserNotFound, $"User '{request.Username}' not found");
            }

            if (post == null)
            {
                throw new RSSNewsReaderException(ErrorCode.PostNotFound, $"Post with id '{request.PostId}' not found");
            }

            if (await _dbContext.ReadPosts.AnyAsync(rp => rp.UserId == user.Id && rp.PostId == post.Id, cancellationToken))
            {
                return new()
                {
                    IsRead = true
                };
            }

            ReadPost readPost = new()
            {
                UserId = user.Id,
                PostId = post.Id
            };

            _ = await _dbContext.AddAsync(readPost, cancellationToken);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new()
            {
                IsRead = true
            };
        }
    }
}