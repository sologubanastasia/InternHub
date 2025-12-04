using InternHub.Application.DTO.Candidate;

using InternHub.Application.DTO.Auth;
namespace InternHub.Application.Services.Candidate
{
    public interface ICandidateService
    {
        Task RegisterAsync(RegisterCandidateDto dto);
        Task<bool> LoginAsync(string email, string password);

        Task<CandidateResponseDto> GetProfileAsync(Guid userId);
        Task UpdateProfileAsync(Guid userId, UpdateCandidateDto dto);
        Task DeleteProfileAsync(Guid userId);

        Task UploadResumeAsync(Guid userId, string resumeUrl);
        Task DeleteResumeAsync(Guid userId);
        Task UploadVideoAsync(Guid userId, string videoUrl);
        Task DeleteVideoAsync(Guid userId);

        Task<IEnumerable<JobListDto>> GetJobsAsync();
        Task<JobDetailDto> GetJobByIdAsync(Guid jobId);
        Task ApplyToJobAsync(Guid userId, Guid jobId);
    }
}
