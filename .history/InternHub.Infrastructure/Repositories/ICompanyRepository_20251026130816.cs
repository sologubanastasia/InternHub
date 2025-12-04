using InternHub.Domain;

namespace InternHub.Infrastructure.Repositories
{
    public interface ICompanyRepository
    {
        Task<Company> GetByUserIdAsync(Guid id);
        Task<Company> GetByCompanyIdAsync(Guid id);
        Task AddAsync(Company company);
        Task UpdateAsync(Company company);
        Task DeleteAsync(Company company);
        Task<IEnumerable<Company>> GetAllAsync();
    }
}    