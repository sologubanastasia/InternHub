using AirbnbSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AirbnbSystem.Infrastructure.Services
{
    public interface IUserManager
    {
        Task<IdentityResult> CreateAsync(User user, string password);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<User?> FindByEmailAsync(string email);
        Task<IList<string>> GetRolesAsync(User user);
        Task<IdentityResult> AddToRoleAsync(User user, string role);
    }
}