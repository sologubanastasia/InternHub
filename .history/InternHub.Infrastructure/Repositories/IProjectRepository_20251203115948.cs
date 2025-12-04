using InternHub.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InternHub.Infrastructure.Repositories
{
    public interface IProjectRepository
    {
        // ВИПРАВЛЕНО: Тепер включає ProjectTechnologies для UpdateProjectAsync
        Task<Project?> GetByIdAsync(Guid id); 
        
        Task AddAsync(Project entity);
        
        Task<bool> IsProjectOwnerAsync(Guid projectId, Guid ownerId);
        
        Task SaveChangesAsync();
        
        Task Update(Project entity);
        
        // Використовується для GET /api/projects/my
        IQueryable<Project> GetProjectsQuery(Guid ownerId); 
        
        IQueryable<Project> GetActiveProjectsQuery();
        
        // Використовується для GET /api/projects/{id}
        Task<Project?> GetProjectDetailsByIdAsync(Guid projectId);
    }
}