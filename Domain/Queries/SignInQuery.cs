using System.Data.Entity;
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
        public string Login { get; set; }
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
        private readonly IPasswordHelper _passwordHelper;

        public SignInQueryHandler(RSSNewsDbContext dbContext, IPasswordHelper passwordHelper)
        {
            _dbContext = dbContext;
            _passwordHelper = passwordHelper;
        }

        public async Task<SignInQueryResult> Handle(SignInQuery request, CancellationToken cancellationToken = default)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Login || u.Email == request.Login, cancellationToken);

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
                    SignInIsSuccessful = true,
                    LoginExists = true,
                    PasswordIsCorrect = true
                })
                : (new()
                {
                    SignInIsSuccessful = false,
                    LoginExists = true,
                    PasswordIsCorrect = false
                });
        }
    }
}