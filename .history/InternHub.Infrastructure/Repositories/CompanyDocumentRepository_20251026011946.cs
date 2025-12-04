using InternHub.Domain;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public class CompanyDocumentRepository : ICompanyDocumentRepository
    {
        private readonly InternHubDbContext _context;
        public CompanyDocumentRepository(InternHubDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CompanyDocument document)
        {
            await _context.CompanyDocuments.AddAsync(document);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(CompanyDocument document)
        {
            _context.CompanyDocuments.Remove(document);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CompanyDocument>> GetByCompanyIdAsync(Guid companyId)
        {
            return await _context.CompanyDocuments
                .Where(d => d.CompanyId == companyId)
                .ToListAsync();
        }
    }
}
