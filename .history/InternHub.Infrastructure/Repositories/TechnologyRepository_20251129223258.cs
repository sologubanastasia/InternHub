using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public class TechnologyRepository : ITechnologyRepository
    {
        private readonly InternHubDbContext _context;
        public TechnologyRepository(InternHubDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Technology technology)
        {
            await _context.Technologies.AddAsync(technology);
        }

        public void Remove(Technology technology)
        {
            _context.Technologies.Remove(technology);
        }

        public async Task<Technology?> GetByNameAsync(string name)
        {
            return await _context.Technologies
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> AllExistAsync(IEnumerable<Guid> technologyIds)
        {
            int requestedCount = technologyIds.Distinct().Count();

            int existingCount = await _context.Technologies
                .Where(t => technologyIds.Contains(t.Id))
                .CountAsync();

            return requestedCount == existingCount;    
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

}    