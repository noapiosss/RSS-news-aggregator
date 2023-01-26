using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using MediatR;

namespace Domain.Commands
{
    public class MarkAsReadCommand : IRequest<MarkAsReadCommandResult>
    {
        public string Username { get; set; }
        public Post Post { get; set; }
    }

    public class MarkAsReadCommandResult
    {
        public bool IsRead { get; set; }
    }

    internal class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, MarkAsReadCommandResult>
    {
        private readonly RSSNewsDbContext _dbContext;

        public MarkAsReadCommandHandler(RSSNewsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MarkAsReadCommandResult> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
            Post post = await _dbContext.Posts.FirstAsync(p => p.Link == request.Post.Link);

            if (user == null || post == null)
            {
                return new()
                {
                    IsRead = false
                };
            };

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