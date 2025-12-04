using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public interface ITechnologyRepository
    {
        Task AddAsync(Technology technology);
        void Remove(Technology technology);
        // check if unique name technology 
        Task<Technology?> GetByNameAsync(string name);
        Task<bool> AllExistAsync(IEnumerable<Guid> technologyIds);
        Task SaveChangesAsync();
    }
}    