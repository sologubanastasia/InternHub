using InternHub.Domain.Entities;

namespace InternHub.Infrastructure.Repositories
{
    public interface ICandidateRepository
    {
        Task<Candidate> GetByUserIdAsync(Guid id);
        Task UpdateAsync(Candidate candidate);
    }
}    