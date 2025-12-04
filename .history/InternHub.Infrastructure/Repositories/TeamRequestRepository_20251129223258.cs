using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public class TeamRequestRepository : ITeamRequestRepository
    {
        private readonly InternHubDbContext _context;
        public TeamRequestRepository(InternHubDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(TeamRequest entity)
        {
            await _context.TeamRequests.AddAsync(entity);
        }
        public  void Update(TeamRequest entity)
        {
            _context.TeamRequests.Update(entity);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<TeamRequest?> GetByIdAsync(Guid id)
        {
            return await  _context.TeamRequests.FindAsync(id);
        }
        public async Task<List<TeamRequest>> GetRequestsTeamIdAsync(Guid projectId)    
        {
            return await _context.TeamRequests
                .Where(tr => tr.ProjectId == projectId)
                .Include(tr => tr.Candidate)
                .ThenInclude(c => c.User)
                .ToListAsync();
        }
        public async Task<bool> HasCandidateApliedAsync(Guid projectId, Guid candidateId)
        {
            return await _context.TeamRequests
                .AnyAsync(tr => tr.ProjectId == projectId &&
                                tr.CandidateId == candidateId &&
                                tr.Status == ApplicationStatus.Pending);
        }
    }

}    