using Microsoft.EntityFrameworkCore;

using System.Threading;
using System.Threading.Tasks;

using Contracts.Database;
using Domain.Database;

using Domain.Helpers.Interfaces;

using MediatR;
using Domain.Base;
using Microsoft.Extensions.Logging;
using Domain.Excetions;
using Contracts.Http;

namespace Domain.Commands
{
    public class CreateUserCommand : IRequest<CreateUserCommandResult>
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class CreateUserCommandResult
    {
        public bool RegistrationIsSuccessful { get; set; }
    }

    internal class CreateUserCommandHandler : BaseHandler<CreateUserCommand, CreateUserCommandResult>
    {
        private readonly RSSNewsDbContext _dbContext;
        private readonly IPasswordHelper _passwordHelper;

        public CreateUserCommandHandler(RSSNewsDbContext dbContext,
            IPasswordHelper passwordHelper,
            ILogger<CreateUserCommandHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
            _passwordHelper = passwordHelper;
        }

        protected override async Task<CreateUserCommandResult> HandleInternal(CreateUserCommand request, CancellationToken cancellationToken = default)
        {
            bool usernameIsAlreadyInUse = await _dbContext.Users.AnyAsync(u => u.Username == request.Username, cancellationToken);
            bool emailIsAlreadyInUse = await _dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);

            if (usernameIsAlreadyInUse)
            {
                throw new RSSNewsReaderException(ErrorCode.UsernameIsAlreadyInUse, $"Username '{request.Username}' is already in use");
            }

            if (emailIsAlreadyInUse)
            {
                throw new RSSNewsReaderException(ErrorCode.EmailIsAlreadyInUse, $"Email '{request.Email}' is already in use");
            }

            User user = new()
            {
                Username = request.Username,
                Email = request.Email,
                Password = _passwordHelper.GetSHA256(request.Password)
            };

            _ = await _dbContext.AddAsync(user, cancellationToken);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new()
            {
                RegistrationIsSuccessful = true
            };
        }
    }
}