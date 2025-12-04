using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace InternHub.Infrastructure.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.Parse(userId ?? throw new Exception("User ID not found in token"));
        }

        public string? GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.Role);
        }
    }
}
