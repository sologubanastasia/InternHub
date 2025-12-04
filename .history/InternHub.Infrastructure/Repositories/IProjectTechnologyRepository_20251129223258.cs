using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public interface IProjectTechnologyRepository
    {
        Task AddRangeAsync(IEnumerable<ProjectTechnology> projectTechnologies);
        Task SaveChangesAsync();
    }

}    