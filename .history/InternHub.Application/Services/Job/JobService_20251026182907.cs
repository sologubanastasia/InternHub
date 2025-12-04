using InternHub.Application.DTO.Job;
using InternHub.Domain;
using InternHub.Infrastructure.Repositories;

namespace InternHub.Application.Services.Job
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly ICandidateRepository _candidateRepository;

        public JobService(IJobRepository jobRepository, IApplicationRepository applicationRepository, ICandidateRepository candidateRepository)
        {
            _jobRepository = jobRepository;
            _applicationRepository = applicationRepository;
            _candidateRepository = candidateRepository;
        }

        public async Task<IEnumerable<JobListDto>> GetAllJobsAsync()
        {
            var jobs = await _jobRepository.GetAllAsync();
            return jobs.Select(j => new JobListDto
            {
                Id = j.Id,
                Title = j.Title,
                Location = j.Location,
                CompanyName = j.Company.CompanyName,
                IsActive = j.IsActive,
                CreatedDate = j.CreatedDate
            });
        }

        public async Task<JobDetailDto> GetJobByIdAsync(Guid id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null) throw new Exception("Job not found.");

            return new JobDetailDto
            {
                Id = job.Id,
                Title = job.Title,
                Requirements = job.Requirements,
                Location = job.Location,
                CompanyName = job.Company.CompanyName,
                CompanyDescription = job.Company.Description ?? "",
                CreatedDate = job.CreatedDate,
                IsActive = job.IsActive
            };
        }

        public async Task ApplyToJobAsync(Guid jobId, Guid candidateUserId, JobApplicationCreateDto dto)
        {
            var candidate = await _candidateRepository.GetByUserIdAsync(candidateUserId);
            if (candidate == null) throw new Exception("Candidate not found.");

            var application = new Application
            {
                Id = Guid.NewGuid(),
                CandidateId = candidate.Id,
                JobId = jobId,
                Message = dto.Message,
                CreatedDate = DateTime.UtcNow,
                Status = ApplicationStatus.Pending
            };

            await _applicationRepository.ApplyAsync(application);
        }
    }
}
