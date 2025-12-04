using InternHub.Domain.Entities;

namespace InternHub.Infrastructure.Services
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
}