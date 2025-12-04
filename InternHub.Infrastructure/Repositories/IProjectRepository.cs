using InternHub.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InternHub.Infrastructure.Repositories
{
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(Guid id); 
        Task AddAsync(Project entity);
        Task<bool> IsProjectOwnerAsync(Guid projectId, Guid ownerId);
        Task SaveChangesAsync();
        Task Update(Project entity);
        IQueryable<Project> GetProjectsQuery(Guid ownerId); 
        IQueryable<Project> GetActiveProjectsQuery();
        Task<Project?> GetProjectDetailsByIdAsync(Guid projectId);
    }
}