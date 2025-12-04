using InternHub.Domain;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly InternHubDbContext _context;

        public JobRepository(InternHubDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Job>> GetAllAsync()
        {
            return await _context.Jobs.Include(j => j.Company).ToListAsync();
        }

        public async Task<Job> GetByIdAsync(Guid id)
        {
            return await _context.Jobs.Include(j => j.Company)
                .Include(j => j.Applications)
                .ThenInclude(a => a.Candidate)
                .FirstOrDefaultAsync(j => j.Id == id);
        }
        
        public async Task AddAsync(Job job)
        {
            await _context.Jobs.AddAsync(job);
            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAsync(Job job)
        {
             _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
        }
        
        public async Task<IEnumerable<Job>> GetByCompanyIdAsync(Guid id)
        {
            return await _context.Jobs
                .Where(j => j.CompanuId == id)
                .Include(j => j.Company)
                .ToListAsync();
        }
    }
}    