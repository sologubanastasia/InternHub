using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;
using InternHub.Application.Interfaces;
using InternHub.Domain;
using InternHub.Infrastructure.Repositories;

namespace InternHub.Application.Services.Company
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ICompanyDocumentRepository _docRepository;
        private readonly IAuthRepository _authRepository;

        public CompanyService(
            ICompanyRepository companyRepository,
            IJobRepository jobRepository,
            ICompanyDocumentRepository docRepository,
            IAuthRepository authRepository)
        {
            _companyRepository = companyRepository;
            _jobRepository = jobRepository;
            _docRepository = docRepository;
            _authRepository = authRepository;
        }

        public async Task<CompanyResponseDto> GetProfileAsync(Guid userId)
        {
            var company = await _companyRepository.GetByUserIdAsync(userId);
            if (company == null) throw new Exception("Company not found.");

            return new CompanyResponseDto
            {
                Id = company.Id,
                CompanyName = company.CompanyName,
                Email = company.User.Email,
                Website = company.Website,
                Description = company.Description,
                Status = company.Status.ToString()
            };
        }

        public async Task UpdateProfileAsync(Guid userId, UpdateCompanyDto dto)
        {
            var company = await _companyRepository.GetByUserIdAsync(userId);
            if (company == null) throw new Exception("Company not found.");

            company.CompanyName = dto.CompanyName;
            company.Description = dto.Description;
            company.Website = dto.Website;
            company.User.Email = dto.Email;

            await _companyRepository.UpdateAsync(company);
        }

        public async Task DeleteProfileAsync(Guid userId)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null) throw new Exception("User not found.");
            await _authRepository.DeleteUserAsync(user);
        }

        public async Task<IEnumerable<CompanyJobListDto>> GetCompanyJobsAsync(Guid userId)
        {
            var company = await _companyRepository.GetByUserIdAsync(userId);
            var jobs = await _jobRepository.GetByCompanyIdAsync(company.Id);

            return jobs.Select(j => new CompanyJobListDto
            {
                Id = j.Id,
                Title = j.Title,
                CompanyName = company.CompanyName,
                IsActive = j.IsActive,
                ApplicantsCount = j.Applications?.Count ?? 0
            });
        }

        public async Task CreateJobAsync(Guid userId, CreateJobDto dto)
        {
            var company = await _companyRepository.GetByUserIdAsync(userId);
            if (company == null) throw new Exception("Company not found.");

            var job = new Job
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Requirements = dto.Requirements,
                Location = dto.Location,
                CompanyId = company.Id,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _jobRepository.AddAsync(job);
        }

        public async Task<IEnumerable<JobApplicationResponseDto>> GetJobApplicationsAsync(Guid jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            return job.Applications.Select(a => new JobApplicationResponseDto
            {
                Id = a.Id,
                JobTitle = job.Title,
                CompanyName = job.Company.CompanyName,
                AppliedDate = a.CreatedDate,
                Status = a.Status.ToString()
            });
        }

        public async Task DeleteJobAsync(Guid jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) throw new Exception("Job not found.");
            await _jobRepository.DeleteAsync(job);
        }

        public async Task UploadDocumentAsync(Guid userId, CompanyDocumentUploadDto dto)
        {
            var company = await _companyRepository.GetByUserIdAsync(userId);
            var doc = new CompanyDocument
            {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                FileName = dto.FileName,
                FileUrl = dto.FileUrl
            };
            await _docRepository.AddAsync(doc);
        }
    }
}
