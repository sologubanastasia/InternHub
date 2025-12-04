using AutoMapper;
using InternHub.Domain;
using InternHub.Infrastructure.Repositories;
using InternHub.Application.DTO.Candidate;
using InternHub.Application.DTO.Job;

namespace InternHub.Application.Services.Candidate
{
    public class CandidateService : ICandidateService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IMapper _mapper;

        public CandidateService(
            IAuthRepository authRepository,
            ICandidateRepository candidateRepository,
            IApplicationRepository applicationRepository,
            IJobRepository jobRepository,
            IMapper mapper)
        {
            _authRepository = authRepository;
            _candidateRepository = candidateRepository;
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
            _mapper = mapper;
        }

        public async Task RegisterAsync(RegisterCandidateDto dto)
        {
            var user = _mapper.Map<ApplicationUser>(dto);
            user.Candidate = new Candidate();
            await _authRepository.RegisterUserAsync(user);
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            return await _authRepository.ValidateUserCredentialAsync(email, password);
        }

        public async Task<CandidateResponseDto> GetProfileAsync(Guid userId)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            return _mapper.Map<CandidateResponseDto>(candidate);
        }

        public async Task UpdateProfileAsync(Guid userId, UpdateCandidateDto dto)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            _mapper.Map(dto, candidate);
            await _candidateRepository.UpdateAsync(candidate);
        }

        public async Task DeleteProfileAsync(Guid userId)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
        }

        public async Task UploadResumeAsync(Guid userId, string resumeUrl)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            candidate.ResumeUrl = resumeUrl;
            await _candidateRepository.UpdateAsync(candidate);
        }

        public async Task DeleteResumeAsync(Guid userId)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            candidate.ResumeUrl = null;
            await _candidateRepository.UpdateAsync(candidate);
        }

        public async Task UploadVideoAsync(Guid userId, string videoUrl)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            candidate.VideoUrl = videoUrl;
            await _candidateRepository.UpdateAsync(candidate);
        }

        public async Task DeleteVideoAsync(Guid userId)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            candidate.VideoUrl = null;
            await _candidateRepository.UpdateAsync(candidate);
        }

        public async Task<IEnumerable<JobListDto>> GetJobsAsync()
        {
            var jobs = await _jobRepository.GetAllAsync();
            return jobs.Select(j => _mapper.Map<JobListDto>(j));
        }

        public async Task<JobDetailDto> GetJobByIdAsync(Guid jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            return _mapper.Map<JobDetailDto>(job);
        }

        public async Task ApplyToJobAsync(Guid userId, Guid jobId)
        {
            var application = new Application
            {
                Id = Guid.NewGuid(),
                CandidateId = userId,
                JobId = jobId,
                AppliedDate = DateTime.UtcNow,
                Status = ApplicationStatus.Pending
            };

            await _applicationRepository.ApplyAsync(application);
        }
    }
}
