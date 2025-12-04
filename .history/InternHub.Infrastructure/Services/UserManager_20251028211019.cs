using AirbnbSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AirbnbSystem.Infrastructure.Services
{
     public class UserManager : IUserManager
    {
        private readonly UserManager<User> _userManager;

        public UserManager(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public Task<User?> FindByEmailAsync(string email)
            => _userManager.FindByEmailAsync(email);

        public Task<IdentityResult> CreateAsync(User user, string password)
            => _userManager.CreateAsync(user, password);

        public Task<bool> CheckPasswordAsync(User user, string password)
            => _userManager.CheckPasswordAsync(user, password);

        public Task<IList<string>> GetRolesAsync(User user)
            => _userManager.GetRolesAsync(user);

        public Task<IdentityResult> AddToRoleAsync(User user, string role)
            => _userManager.AddToRoleAsync(user, role);
    }
}    