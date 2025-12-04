using InternHub.Domain;

namespace InternHub.Infrastructure.Repositories
{
    public interface IApplicationRepository
    {
        Task ApplyAsync(Application application);
        Task<IEnumerable<Application>> GetApplicationsByJobIdAsync(Guid jobId);
    }
}    