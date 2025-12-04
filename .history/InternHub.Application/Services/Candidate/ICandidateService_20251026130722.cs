using InternHub.Application.DTO.Candidate;

namespace InternHub.Application.Services.Candidate
{
    public interface ICandidateService
    {
        Task RegisterAsync(RegisterCandidateDto dto);
        Task<bool> LoginAsync(string email, string password);

        Task<CandidateResponseDto> GetProfileAsync(Guid ApplicationUserId);
        Task UpdateProfileAsync(Guid ApplicationUserId, UpdateCandidateDto dto);
        Task DeleteProfileAsync(Guid ApplicationUserId);

        Task UploadResumeAsync(Guid ApplicationUserId, string resumeUrl);
        Task DeleteResumeAsync(Guid ApplicationUserId);
        Task UploadVideoAsync(Guid ApplicationUserId, string videoUrl);
        Task DeleteVideoAsync(Guid ApplicationUserId);

        Task<IEnumerable<JobListDto>> GetJobsAsync();
        Task<JobDetailDto> GetJobByIdAsync(Guid jobId);
        Task ApplyToJobAsync(Guid ApplicationUserId, Guid jobId);
    }
}
