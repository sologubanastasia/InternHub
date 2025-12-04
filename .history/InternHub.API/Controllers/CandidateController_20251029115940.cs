using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly InternHubDbContext _context;

        public CandidateRepository(InternHubDbContext context)
        {
            _context = context;
        }

        public async Task<Candidate> GetByUserIdAsync(Guid userId)
        {
            return await _context.Candidates
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.User.Id == userId);
        }

        public async Task UpdateAsync(Candidate candidate)
        {
            _context.Candidates.Update(candidate);
            await _context.SaveChangesAsync();
        }
    }
}
