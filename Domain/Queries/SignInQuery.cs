using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using Domain.Helpers.Interfaces;
using MediatR;
using Domain.Base;
using Domain.Excetions;
using Contracts.Http;
using Microsoft.Extensions.Logging;

namespace Domain.Queries
{
    public class SignInQuery : IRequest<SignInQueryResult>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SignInQueryResult
    {
        public bool IsAuthenticated { get; set; }
    }

    internal class SignInQueryHandler : BaseHandler<SignInQuery, SignInQueryResult>
    {
        private readonly RSSNewsDbContext _dbContext;
        private readonly IPasswordHelper _passwordHelper;

        public SignInQueryHandler(RSSNewsDbContext dbContext, IPasswordHelper passwordHelper, ILogger<SignInQuery> logger) : base(logger)
        {
            _dbContext = dbContext;
            _passwordHelper = passwordHelper;
        }

        protected override async Task<SignInQueryResult> HandleInternal(SignInQuery request, CancellationToken cancellationToken = default)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

            return user == null ? throw new RSSNewsReaderException(ErrorCode.UserNotFound, $"User '{request.Username}' not found")
                : _passwordHelper.GetSHA256(request.Password) != user.Password ? throw new RSSNewsReaderException(ErrorCode.WrongPassword, $"Wrong password")
                : (new()
                {
                    IsAuthenticated = true
                });
        }
    }
}