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
            return await _context.Users
                .Include(u => u.Candidate)
                .Include(u => u.Company)
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.Candidate)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Candidate)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task RegisterUserAsync(ApplicationUser user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ValidateUserCredentialAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;
            return true;
        }

        public async Task DeleteUserAsync(ApplicationUser user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
