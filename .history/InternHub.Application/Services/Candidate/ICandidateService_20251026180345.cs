using InternHub.Application.DTO.Candidate;

namespace InternHub.Application.Interfaces
{
    public interface ICandidateService
    {
        Task<CandidateResponseDto> GetProfileAsync(Guid userId);
        Task UpdateProfileAsync(Guid userId, UpdateCandidateDto dto);
        Task DeleteProfileAsync(Guid userId);

        Task UpdateResumeAsync(Guid userId, UploadResumeDto dto);
        Task DeleteResumeAsync(Guid userId);

        Task UpdateVideoAsync(Guid userId, UploadVideoDto dto);
        Task DeleteVideoAsync(Guid userId);
    }
}
