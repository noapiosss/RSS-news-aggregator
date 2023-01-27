using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Domain.Database;
using Domain.Helpers.Interfaces;
using MediatR;

namespace domain.Queries
{
    public class SignInQuery : IRequest<SignInQueryResult>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SignInQueryResult
    {
        public bool SignInIsSuccessful { get; set; }
        public bool LoginExists { get; set; }
        public bool PasswordIsCorrect { get; set; }
    }

    internal class SignInQueryHandler : IRequestHandler<SignInQuery, SignInQueryResult>
    {
        private readonly RSSNewsDbContext _dbContext;
        private readonly ISHA256 _passwordHelper;

        public SignInQueryHandler(RSSNewsDbContext dbContext, ISHA256 passwordHelper)
        {
            _dbContext = dbContext;
            _passwordHelper = passwordHelper;
        }

        public async Task<SignInQueryResult> Handle(SignInQuery request, CancellationToken cancellationToken = default)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

            return user == null
                ? (new()
                {
                    SignInIsSuccessful = false,
                    LoginExists = false,
                    PasswordIsCorrect = false
                })
                : _passwordHelper.GetSHA256(request.Password) != user.Password
                ? (new()
                {
                    SignInIsSuccessful = false,
                    LoginExists = true,
                    PasswordIsCorrect = false
                })
                : (new()
                {
                    SignInIsSuccessful = true,
                    LoginExists = true,
                    PasswordIsCorrect = true
                });
        }
    }
}