using AirbnbSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AirbnbSystem.Infrastructure.Services
{
    public interface IUserManager
    {
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<IList<string>> GetRolesAsync(User user);
        Task<IdentityResult> AddToRoleAsync(User user, string role);
    }
}