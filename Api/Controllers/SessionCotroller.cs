using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Http;
using domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class SessionCotroller : ControllerBase
    {
        private readonly IMediator _mediator;

        public SessionCotroller(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SignInQuery signInQuery = new()
            {
                Username = request.Username,
                Password = request.Password
            };

            SignInQueryResult signInQueryResult = await _mediator.Send(signInQuery, cancellationToken);
            SignInResponse signInResponse = new()
            {
                SignInIsSuccessful = signInQueryResult.SignInIsSuccessful,
                LoginExists = signInQueryResult.LoginExists,
                PasswordIsCorrect = signInQueryResult.PasswordIsCorrect
            };

            if (!signInResponse.SignInIsSuccessful)
            {
                return BadRequest(Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest);
            }

            Claim[] claims = new[]
            {
                new Claim(ClaimTypes.Name, request.Username)
            };

            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal claimsPrincipal = new(claimsIdentity);
            AuthenticationProperties authProperties = new();

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties);

            return Ok();

        }

        [HttpGet("sign-out")]
        public async Task<IActionResult> SignOut(CancellationToken cancellationToken)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}