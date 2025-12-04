using System;
using System.Threading.Tasks;
using InternHub.Domain.Entities;

namespace InternHub.Infrastructure.Repositories
{
    public interface IProjectMemberRepository
    {
        // МЕТОД, ЯКИЙ ВИКЛИКАВ ПОМИЛКУ CS0535
        Task<ProjectMember?> GetProjectMemberIdAsync(Guid id);
        
        // НОВИЙ МЕТОД, ЯКИЙ ПОТРІБЕН СЕРВІСУ
        Task<ProjectMember?> GetMemberByCandidateAndProjectAsync(Guid projectId, Guid candidateId);
        
        Task AddAsync(ProjectMember entity);
        Task Remove(Guid projectId, Guid memberId);
        
        Task<bool> IsMemberInProjectAsync(Guid projectId, Guid userId);
    }
}