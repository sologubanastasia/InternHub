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

        // НОВИЙ МЕТОД: Пошук члена за CandidateId та ProjectId. (Коректно)
        public Task<ProjectMember?> GetMemberByCandidateAndProjectAsync(Guid projectId, Guid candidateId)
        {
            return _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.CandidateId == candidateId);
        }

        public async Task AddAsync(ProjectMember entity)
        {
            await _context.ProjectMembers.AddAsync(entity);
        }

        // ✅ ДОДАНО: Перевантаження, щоб реалізувати Remove(ProjectMember entity)
        public Task Remove(ProjectMember entity)
        {
            if (entity != null)
            {
                _context.ProjectMembers.Remove(entity);
            }
            return Task.CompletedTask;
        }

        // Існуючий метод видалення за ID
        public async Task Remove(Guid projectId, Guid memberId)
        {
            var entity = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.CandidateId == memberId);

            if (entity != null)
            {
                _context.ProjectMembers.Remove(entity);
            }
        }

        public async Task<bool> IsMemberInProjectAsync(Guid projectId, Guid userId)
        {
            return await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.CandidateId == userId);
        }
    }
}