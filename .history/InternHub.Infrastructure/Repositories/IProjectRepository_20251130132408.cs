using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace InternHub.Infrastructure.Repositories
{
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(Guid id);
        Task AddAsync(Project entity);
        Task SaveChangesAsync();
        Task Update(Project entity);
        IQueryable<Project> GetProjectsQuery(Guid ownerId);
        Task<bool> IsProjectOwnerAsync(Guid projectId, Guid ownerId);
        Task<Project?> GetProjectDetailsByIdAsync(Guid projectId);
        IQueryable<Project> GetActiveProjectsQuery();
    }

}    