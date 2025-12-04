using InternHub.Domain;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly InternHubDbContext _context;

        public CompanyRepository(InternHubDbContext context)
        {
            _context = context;
        }

        public async Task<Company> GetByApplicationUserIdAsync(Guid id)
        {
            return await _context.Companies.Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        
        public async Task<Company> GetByCompanyIdAsync(Guid id)
        {
            return await _context.Companies.Include(c => c.ApplicationUser)
                .Include(c => c.Jobs)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Company company)
        {
            await _context.Companies.AddAsync(company);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Company company)
        {
             _context.Companies.Update(company);
             await _context.SaveChangesAsync;
        }

        public async Task DeleteAsync(Company company)
        {
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Company>> GetAllAsync()
        {
            return await _context.Companies.Include(c => c.ApplicationUser).ToListAsync();
        }
    }
}    