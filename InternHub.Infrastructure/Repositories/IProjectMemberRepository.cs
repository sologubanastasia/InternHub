using System;
using System.Threading.Tasks;
using InternHub.Domain.Entities;

namespace InternHub.Infrastructure.Repositories
{
public interface IProjectMemberRepository
{
    // 💡 ЗАМІНІТЬ застарілий метод на GetByIdAsync:
    Task<ProjectMember?> GetByIdAsync(Guid id); // <-- ЦЕ ВИПРАВЛЕННЯ

    Task<ProjectMember?> GetMemberByCandidateAndProjectAsync(Guid projectId, Guid candidateId);
    
    Task AddAsync(ProjectMember entity);
    Task Remove(ProjectMember entity); 
    
    Task Remove(Guid projectId, Guid memberId);
    Task<bool> IsMemberInProjectAsync(Guid projectId, Guid userId);
    Task SaveChangesAsync();
}
}