using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace InternHub.Infrastructure.Repositories
{
    public class ProjectMemberRepository : IProjectMemberRepository
    {
        private readonly InternHubDbContext _context;

        public ProjectMemberRepository(InternHubDbContext context)
        {
            _context = context;
        }

        // ✅ УЗГОДЖЕНО з інтерфейсом: GetByIdAsync
        public Task<ProjectMember?> GetByIdAsync(Guid id)
        {
            return _context.ProjectMembers.FindAsync(id).AsTask();
        }

        // ✅ УЗГОДЖЕНО з інтерфейсом: GetMemberByCandidateAndProjectAsync
        public Task<ProjectMember?> GetMemberByCandidateAndProjectAsync(Guid projectId, Guid candidateId)
        {
            return _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.CandidateId == candidateId);
        }

        // ✅ УЗГОДЖЕНО з інтерфейсом: AddAsync
        public async Task AddAsync(ProjectMember entity)
        {
            await _context.ProjectMembers.AddAsync(entity);
        }

        // ✅ УЗГОДЖЕНО з інтерфейсом: Remove(ProjectMember entity)
        public Task Remove(ProjectMember entity)
        {
            if (entity != null)
            {
                _context.ProjectMembers.Remove(entity);
            }
            return Task.CompletedTask;
        }

        // ✅ УЗГОДЖЕНО з інтерфейсом: Remove(Guid projectId, Guid memberId)
        public async Task Remove(Guid projectId, Guid memberId)
        {
            var entity = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.CandidateId == memberId);

            if (entity != null)
            {
                _context.ProjectMembers.Remove(entity);
            }
        }

        // ✅ УЗГОДЖЕНО з інтерфейсом: IsMemberInProjectAsync
        public async Task<bool> IsMemberInProjectAsync(Guid projectId, Guid userId)
        {
            return await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.CandidateId == userId);
        }

        // 🟢 ВИПРАВЛЕННЯ: Реалізація SaveChangesAsync, доданого до інтерфейсу
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}