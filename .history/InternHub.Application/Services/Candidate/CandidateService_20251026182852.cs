using InternHub.Application.DTO.Candidate;
using InternHub.Infrastructure.Repositories;

namespace InternHub.Application.Services.Candidate
{
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IAuthRepository _authRepository;

        public CandidateService(ICandidateRepository candidateRepository, IAuthRepository authRepository)
        {
            _candidateRepository = candidateRepository;
            _authRepository = authRepository;
        }

        public async Task<CandidateResponseDto> GetProfileAsync(Guid userId)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            if (candidate == null) throw new Exception("Candidate not found.");

            return new CandidateResponseDto
            {
                Id = candidate.Id,
                GitHubUrl = candidate.GitHubUrl,
                Email = candidate.User.Email,
                Telegram = candidate.Telegram,
                ResumeUrl = candidate.ResumeUrl,
                VideoUrl = candidate.VideoUrl
            };
        }

        public async Task UpdateProfileAsync(Guid userId, UpdateCandidateDto dto)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            if (candidate == null) throw new Exception("Candidate not found.");

            candidate.GitHubUrl = dto.GitHubUrl;
            candidate.Telegram = dto.Telegram;
            candidate.User.Email = dto.Email ?? candidate.User.Email;

            await _candidateRepository.UpdateAsync(candidate);
        }

        public async Task DeleteProfileAsync(Guid userId)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null) throw new Exception("User not found.");

            await _authRepository.DeleteUserAsync(user);
        }

        public async Task UpdateResumeAsync(Guid userId, UploadResumeDto dto)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            candidate.ResumeUrl = dto.ResumeUrl;
            await _candidateRepository.UpdateAsync(candidate);
        }

        public async Task DeleteResumeAsync(Guid userId)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            candidate.ResumeUrl = null;
            await _candidateRepository.UpdateAsync(candidate);
        }

        public async Task UpdateVideoAsync(Guid userId, UploadVideoDto dto)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            candidate.VideoUrl = dto.VideoUrl;
            await _candidateRepository.UpdateAsync(candidate);
        }

        public async Task DeleteVideoAsync(Guid userId)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            candidate.VideoUrl = null;
            await _candidateRepository.UpdateAsync(candidate);
        }
    }
}
