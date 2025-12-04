using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace InternHub.Infrastructure.Repositories
{
    public interface ITechnologyRepository
    {
        Task AddAsync(Technology technology);
        Task DeleteAsync(Technology technology);
        Task<Technology?> GetByNameAsync(string name);
        Task<Technology> GetByIdAsync(Guid id);
        IQueryable<Technology> GetTechnologyQuery();
        Task<bool> AllExistAsync(IEnumerable<Guid> technologyIds);
        Task SaveChangesAsync();
    }
}    