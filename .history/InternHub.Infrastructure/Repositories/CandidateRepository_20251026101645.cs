using InternHub.Domain;
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

        public async  Task<Candidate> GetByUserIdAsync(Guid id)
        {
            return await  _context.Candidates
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateAsync(Candidate candidate)
        {
            _context.Candidates.Update(candidate);
            return await _context.SaveChangesAsync();
        }
    }
}    
        