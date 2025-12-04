using InternHub.Domain;

namespace InternHub.Infrastructure.Repositories
{
    public interface ICandidateRepository
    {
        Task<Candidate> GetByApplicationUserIdAsync(Guid id);
        Task UpdateAsync(Candidate candidate);
    }
}    