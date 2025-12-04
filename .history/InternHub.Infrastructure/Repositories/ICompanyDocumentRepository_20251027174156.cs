using InternHub.Domain.Entities;

namespace Infrastructure.Repositories
{
    public interface ICompanyDocumentRepository
    {
        Task AddAsync(CompanyDocument document);
        Task DeleteAsync(CompanyDocument document);
        Task<IEnumerable<CompanyDocument>> GetByCompanyIdAsync(Guid companyId);
    }
}
