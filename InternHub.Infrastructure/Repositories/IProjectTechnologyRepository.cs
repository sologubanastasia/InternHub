using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternHub.Infrastructure.Repositories
{
    public interface IProjectTechnologyRepository
    {
        Task AddRangeAsync(IEnumerable<ProjectTechnology> projectTechnologies);
        void RemoveRange(IEnumerable<ProjectTechnology> projectTechnologies);
        
        Task SaveChangesAsync();
    }
}