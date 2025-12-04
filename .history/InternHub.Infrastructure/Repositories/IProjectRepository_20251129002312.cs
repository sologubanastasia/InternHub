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
        void UpdateAsync(Project entity);
        void DeleteAsync(Project entity);
        Task SaveChangesAsync();
        IQueryable<Project> GetProjectsQuery(Guid ownerId);
        Task<Project?> GetProjectDetailsByIdAsyn(Guid projectId);
        IQueryable<Project> GetActiveProjectsQuery();
    }

}    