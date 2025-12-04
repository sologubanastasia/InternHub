using InternHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly InternHubDbContext _context;

        public AuthRepository(InternHubDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Candidate)
                .Include(u => u.Company)
                .ToListAsync();

            // Підвантаження ролей для кожного користувача
            foreach (var user in users)
            {
                user.Role = await GetRoleForUserAsync(user.Id);
            }

            return users;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Candidate)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                user.Role = await GetRoleForUserAsync(user.Id);
            }

            return user;
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.Candidate)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                user.Role = await GetRoleForUserAsync(user.Id);
            }

            return user;
        }

        public async Task RegisterUserAsync(ApplicationUser user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ValidateUserCredentialAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user != null; // Тільки перевірка наявності користувача
        }

        public async Task DeleteUserAsync(ApplicationUser user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        // Додаткова приватна функція для отримання ролі користувача
        private async Task<string> GetRoleForUserAsync(Guid userId)
        {
            var roleName = await (from ur in _context.UserRoles
                                  join r in _context.Roles on ur.RoleId equals r.Id
                                  where ur.UserId == userId.ToString()
                                  select r.Name)
                                  .FirstOrDefaultAsync();

            return roleName ?? "User"; // якщо роль не знайдена
        }
    }
}
