using InternHub.Domain;
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

        public async Task<IEnumerable<ApplicationApplicationUser>> GetAllApplicationUsersAsync()
        {
            return await _context.ApplicationUsers
                .Include(u => u.Candidate)
                .Include(u => u.Company)
                .ToListAsync();
        }

        public async Task<ApplicationApplicationUser?> GetApplicationUserByIdAsync(Guid ApplicationUserId)
        {
            return await _context.ApplicationUsers
                .Include(u => u.Candidate)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == ApplicationUserId);
        }

        public async Task<ApplicationApplicationUser?> GetApplicationUserByEmailAsync(string email)
        {
            return await _context.ApplicationUsers
                .Include(u => u.Candidate)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task RegisterApplicationUserAsync(ApplicationApplicationUser ApplicationUser)
        {
            await _context.ApplicationUsers.AddAsync(ApplicationUser);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ValidateApplicationUserCredentialAsync(string email, string password)
        {
            var ApplicationUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (ApplicationUser == null) return false;
            return true;
        }

        public async Task DeleteApplicationUserAsync(ApplicationApplicationUser ApplicationUser)
        {
            _context.ApplicationUsers.Remove(ApplicationUser);
            await _context.SaveChangesAsync();
        }
    }
}
