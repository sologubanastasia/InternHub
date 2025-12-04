using InternHub.Application.DTO.Job;

namespace InternHub.Application.Interfaces
{
    public interface IJobService
    {
        Task<IEnumerable<JobListDto>> GetAllJobsAsync();
        Task<JobDetailDto> GetJobByIdAsync(Guid id);
        Task ApplyToJobAsync(Guid jobId, Guid candidateUserId, JobApplicationCreateDto dto);
    }
}
