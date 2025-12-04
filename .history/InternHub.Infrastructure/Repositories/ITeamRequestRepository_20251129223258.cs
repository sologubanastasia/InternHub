using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Infrastructure.Repositories
{
    public interface ITeamRequestRepository
    {
        Task AddAsync(TeamRequest entity);
        void Update(TeamRequest entity);
        Task SaveChangesAsync();
        Task<TeamRequest?> GetByIdAsync(Guid id);
        Task<List<TeamRequest>> GetRequestsTeamIdAsync(Guid teamId);
        Task<bool> HasCandidateApliedAsync(Guid teamId, Guid candidateId);
    }

}    