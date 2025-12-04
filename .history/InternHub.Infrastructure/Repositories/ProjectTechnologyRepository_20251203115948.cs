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

        // 💡 ВИПРАВЛЕНО CS0738: Змінено сигнатуру на 'public async Task'
        // Тепер вона відповідає 'Task AddRangeAsync(...)' в інтерфейсі.
        public async Task AddRangeAsync(IEnumerable<ProjectTechnology> projectTechnologies)
        {
            // Використовуємо await, щоб коректно обробити ValueTask від EF Core
            await _context.ProjectTechnologies.AddRangeAsync(projectTechnologies);
        }

        // Цей метод коректний, оскільки він синхронний і відповідає 'void RemoveRange(...)'
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