using System;
using System.Threading.Tasks;
using InternHub.Domain.Entities;

namespace InternHub.Infrastructure.Repositories
{
    public interface IProjectMemberRepository
    {
        Task<ProjectMember?> GetProjectMemberIdAsync(Guid id);
        Task AddAsync(ProjectMember entity);
        Task Remove(Guid projectId, Guid memberId);
        Task<bool> IsMemberInProjectAsync(Guid projectId, Guid userId);
    }
}