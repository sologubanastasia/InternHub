using InternHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly InternHubDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public AuthRepository(
            InternHubDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ✅ Отримати всіх користувачів
        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Candidate)
                .Include(u => u.Company)
                .ToListAsync();
        }

        // ✅ Отримати користувача за Id
        public async Task<ApplicationUser?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.Candidate)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        // ✅ Отримати користувача за Email
        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Candidate)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // ✅ Реєстрація користувача з роллю (через Identity)
        public async Task RegisterUserAsync(ApplicationUser user, string password, string roleName)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
            }

            await _userManager.AddToRoleAsync(user, roleName);
        }

        // ✅ Перевірка облікових даних
        public async Task<bool> ValidateUserCredentialAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            return await _userManager.CheckPasswordAsync(user, password);
        }

        // ✅ Видалення користувача
        public async Task DeleteUserAsync(ApplicationUser user)
        {
            await _userManager.DeleteAsync(user);
        }

        // ✅ Отримати ролі користувача
        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }
    }
}
