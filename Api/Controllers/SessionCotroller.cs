using System.Threading;
using System.Threading.Tasks;
using Api.Services.Interfaces;
using Contracts.Http;
using Domain.Commands;
using Domain.Queries;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [Route("api/session")]
    public class SessionCotroller : BaseCotroller
    {
        private readonly IMediator _mediator;
        private readonly IValidator<SignUpRequest> _signUpRequestValidator;
        private readonly ITokenHandler _tokenHandler;

        public SessionCotroller(IMediator mediator,
            IValidator<SignUpRequest> signUpRequestValidator,
            ITokenHandler tokenHandler,
            ILogger<SessionCotroller> logger) : base(logger)
        {
            _mediator = mediator;
            _signUpRequestValidator = signUpRequestValidator;
            _tokenHandler = tokenHandler;
        }


        /// <summary>
        /// Create account
        /// </summary>
        /// <returns>Registration result</returns>
        /// <response code="200">Registration was successful message</response>
        /// <response code="400">Not valid input</response>
        /// <response code="409">Input already in use</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPut("sign-up")]
        public Task<IActionResult> SignUp([FromBody] SignUpRequest request, CancellationToken cancellationToken)
        {
            return SafeExecute(async () =>
            {
                ValidationResult validationResult = await _signUpRequestValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return ToActionResult(new()
                    {
                        Code = ErrorCode.BadRequest,
                        Message = string.Join(" Also, ", validationResult.Errors)
                    });
                }

                CreateUserCommand createUserCommand = new()
                {
                    Username = request.Username,
                    Email = request.Email,
                    Password = request.Password
                };

                CreateUserCommandResult createUserCommandResult = await _mediator.Send(createUserCommand, cancellationToken);
                SignUpResponse signUpResponse = new()
                {
                    RegistrationIsSuccessful = createUserCommandResult.RegistrationIsSuccessful
                };

                return Ok(signUpResponse);

            }, cancellationToken);
        }

        /// <summary>
        /// Sign-in request
        /// </summary>
        /// <returns>Registration result</returns>
        /// <response code="200">Return token</response>
        /// <response code="400">Invalid input</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("sign-in")]
        public Task<IActionResult> SignIn([FromBody] SignInRequest request, CancellationToken cancellationToken)
        {
            return SafeExecute(async () =>
            {
                if (!ModelState.IsValid)
                {
                    return ToActionResult(new()
                    {
                        Code = ErrorCode.BadRequest,
                        Message = "Invalid requset"
                    });
                }

                SignInQuery signInQuery = new()
                {
                    Username = request.Username,
                    Password = request.Password
                };

                SignInQueryResult signInQueryResult = await _mediator.Send(signInQuery, cancellationToken);
                SignInResponse signInResponse = new()
                {
                    IsAuthenticated = signInQueryResult.IsAuthenticated
                };

                string token = _tokenHandler.GenerateToken(request.Username);

                return Ok(token);

            }, cancellationToken);
        }
    }
}