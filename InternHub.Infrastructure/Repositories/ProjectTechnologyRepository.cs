using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternHub.Infrastructure.Repositories
{
    public class ProjectTechnologyRepository : IProjectTechnologyRepository
    {
        private readonly InternHubDbContext _context;

        public ProjectTechnologyRepository(InternHubDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<ProjectTechnology> projectTechnologies)
        {
            await _context.ProjectTechnologies.AddRangeAsync(projectTechnologies);
        }
        
        public void RemoveRange(IEnumerable<ProjectTechnology> projectTechnologies)
        {
            _context.ProjectTechnologies.RemoveRange(projectTechnologies);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}