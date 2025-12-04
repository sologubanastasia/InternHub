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
        void Update(Project entity);
        void Delete(Project entity);
        Task SaveChangesAsync();
        IQueryable<Project> GetProjectsQuery(Guid ownerId);
        Task<Project?> GetProjectDetailsByIdAsync(Guid projectId);
        IQueryable<Project> GetActiveProjectsQuery();
    }

}    