using System.Security.Claims;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Api.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAuthorized(out string username)
        {
            if (_httpContextAccessor != null)
            {
                username = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
                if (username != null)
                {
                    return true;
                }
            }
            username = "";
            return false;
        }
    }
}