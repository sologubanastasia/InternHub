using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly InternHubDbContext _context;

        public ApplicationRepository(InternHubDbContext context)
        {
            _context = context;
        }

         public async Task ApplyAsync(Application application)
        {
            await _context.Applications.AddAsync(application);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Application>> GetApplicationsByJobIdAsync(Guid jobId)
        {
            return await _context.Applications
                .Include(a => a.Candidate)
                .Where(a => a.JobId == jobId)
                .ToListAsync();
        }
        
    }
}    