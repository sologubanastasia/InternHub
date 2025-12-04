using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public class  ProjectMemberRepository : IProjectMemberRepository
    {
        private readonly InternHubDbContext _context;

        public ProjectMemberRepository(InternHubDbContext context)
        {
            _context = context;
        }

        public Task<ProjectMember?> GetProjectMemberIdAsync(Guid id)
        {
            return _context.ProjectMembers.FindAsync(id).AsTask();
        }

        public async Task AddAsync(ProjectMember entity)
        {
            await _context.ProjectMembers.AddAsync(entity);
        }

        public async Task Remove(Guid projectId, Guid memberId)
        {
            var entity = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.CandidateId == memberId);

            if (entity != null)
            {
                _context.ProjectMembers.Remove(entity);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsMemberInProjectAsync(Guid projectId, Guid userId)
        {
            return await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.CandidateId == userId);
        }
    }
}    