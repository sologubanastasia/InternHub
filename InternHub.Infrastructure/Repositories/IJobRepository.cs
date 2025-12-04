using InternHub.Domain.Entities;

namespace InternHub.Infrastructure.Repositories
{
    public interface IJobRepository
    {
        Task<IEnumerable<Job>> GetAllAsync();
        Task<Job> GetByIdAsync(Guid id);
        Task AddAsync(Job job);
        Task DeleteAsync(Job job);
        Task<IEnumerable<Job>> GetByCompanyIdAsync(Guid id);
    }
}    