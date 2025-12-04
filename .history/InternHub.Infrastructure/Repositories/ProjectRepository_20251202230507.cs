using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace InternHub.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly InternHubDbContext _context;

        public ProjectRepository(InternHubDbContext context)
        {
            _context = context;
        }

        public async Task<Project?> GetByIdAsync(Guid id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task AddAsync(Project entity)
        {
            await _context.Projects.AddAsync(entity);
        }

        public async Task<bool> IsProjectOwnerAsync(Guid projectId,Guid ownerId)
        {
            return await _context.Projects
                .AnyAsync(p => p.Id == projectId && p.CandidateId == ownerId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public Task Update(Project entity)
        {
            _context.Projects.Update(entity); 
            return Task.CompletedTask; 
        }

        // ВИПРАВЛЕННЯ: Додано Include для уникнення помилок 500/N+1
        public IQueryable<Project> GetProjectsQuery(Guid ownerId)
        {
            return _context.Projects
                .Where(p => p.CandidateId == ownerId)
                .Include(p => p.ProjectTechnologies).ThenInclude(pt => pt.Technology) // Додано
                .AsQueryable();
        }

        public IQueryable<Project> GetActiveProjectsQuery(){
            return _context.Projects
                .Where(p => p.IsTeamSearchActive && !p.IsComplited)
                .Include(p => p.Candidate).ThenInclude(c => c.User)
                .Include(p => p.ProjectTechnologies).ThenInclude(pt => pt.Technology)
                .Include(p => p.ProjectMembers)
                .AsQueryable();
        }

        public Task<Project?> GetProjectDetailsByIdAsync(Guid projectId)
        {
            return _context.Projects
                .Where(p => p.Id == projectId)
                .Include(p => p.Candidate).ThenInclude(c => c.User)
                .Include(p => p.ProjectTechnologies).ThenInclude(pt => pt.Technology)
                .Include(p => p.ProjectMembers).ThenInclude(pm => pm.Candidate).ThenInclude(c => c.User)
                .FirstOrDefaultAsync();
        }
    }
}