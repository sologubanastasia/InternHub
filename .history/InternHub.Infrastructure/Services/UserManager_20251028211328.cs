using InternHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AirbnbSystem.Infrastructure.Services
{
     public class UserManager : IUserManager
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserManager(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public Task<ApplicationUser?> FindByEmailAsync(string email)
            => _userManager.FindByEmailAsync(email);

        public Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
            => _userManager.CreateAsync(user, password);

        public Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
            => _userManager.CheckPasswordAsync(user, password);

        public Task<IList<string>> GetRolesAsync(ApplicationUser user)
            => _userManager.GetRolesAsync(user);

        public Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
            => _userManager.AddToRoleAsync(user, role);
    }
}    