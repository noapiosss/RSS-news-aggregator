using Contracts.Http;
using FluentValidation;

namespace Api.Validation
{
    internal class SignUpRequestValidator : AbstractValidator<SignUpRequest>
    {
        public SignUpRequestValidator()
        {
            _ = RuleFor(r => r.Username).NotNull().Length(1, 50);
            _ = RuleFor(r => r.Email).EmailAddress();
            _ = RuleFor(r => r.Password).NotNull().Length(8, 50);
        }
    }
}