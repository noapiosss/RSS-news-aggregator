using Microsoft.EntityFrameworkCore;

using System.Threading;
using System.Threading.Tasks;

using Contracts.Database;
using Domain.Database;

using Domain.Helpers.Interfaces;

using MediatR;

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
        public bool IsRegistrationSuccessful { get; set; }
        public bool UsernameIsAlreadyInUse { get; set; }
        public bool EmailIsAlreadyInUse { get; set; }
    }

    internal class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserCommandResult>
    {
        private readonly RSSNewsDbContext _dbContext;
        private readonly ISHA256 _passwordHelper;

        public CreateUserCommandHandler(RSSNewsDbContext dbContext, ISHA256 passwordHelper)
        {
            _dbContext = dbContext;
            _passwordHelper = passwordHelper;
        }

        public async Task<CreateUserCommandResult> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
        {
            bool usernameIsAlreadyInUse = await _dbContext.Users.AnyAsync(u => u.Username == request.Username, cancellationToken);
            bool emailIsAlreadyInUse = await _dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);

            if (usernameIsAlreadyInUse || emailIsAlreadyInUse)
            {
                return new()
                {
                    IsRegistrationSuccessful = false,
                    UsernameIsAlreadyInUse = usernameIsAlreadyInUse,
                    EmailIsAlreadyInUse = emailIsAlreadyInUse
                };
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
                IsRegistrationSuccessful = true,
                UsernameIsAlreadyInUse = usernameIsAlreadyInUse,
                EmailIsAlreadyInUse = emailIsAlreadyInUse
            };
        }
    }
}