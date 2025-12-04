using AutoMapper;
using InternHub.Domain;
using InternHub.Infrastructure.Repositories;
using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;
using InternHub.Application.DTO.ApplicationUser;

namespace InternHub.Application.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;

        public AdminService(
            ICompanyRepository companyRepository,
            IJobRepository jobRepository,
            IAuthRepository authRepository,
            IMapper mapper)
        {
            _companyRepository = companyRepository;
            _jobRepository = jobRepository;
            _authRepository = authRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ApplicationUserResponseDto>> GetAllApplicationUsersAsync()
        {
            var ApplicationUsers = await _authRepository.GetAllApplicationUsersAsync();
            return ApplicationUsers.Select(u => _mapper.Map<ApplicationUserResponseDto>(u));
        }

        public async Task DeleteApplicationUserAsync(Guid ApplicationUserId)
        {
            var ApplicationUser = await _authRepository.GetApplicationUserByIdAsync(ApplicationUserId);
            if (ApplicationUser != null)
            {
                await _authRepository.DeleteApplicationUserAsync(ApplicationUser);
            }
        }

        public async Task<IEnumerable<CompanyResponseDto>> GetAllCompaniesAsync()
        {
            var companies = await _companyRepository.GetAllAsync();
            return companies.Select(c => _mapper.Map<CompanyResponseDto>(c));
        }

        public async Task ApproveCompanyAsync(Guid companyId)
        {
            var company = await _companyRepository.GetByCompanyIdAsync(companyId);
            if (company != null)
            {
                company.Status = CompanyStatus.Approved;
                await _companyRepository.UpdateAsync(company);
            }
        }

        public async Task DeclineCompanyAsync(Guid companyId)
        {
            var company = await _companyRepository.GetByCompanyIdAsync(companyId);
            if (company != null)
            {
                company.Status = CompanyStatus.NotApproved;
                await _companyRepository.UpdateAsync(company);
            }
        }

        public async Task<IEnumerable<JobListDto>> GetAllJobsAsync()
        {
            var jobs = await _jobRepository.GetAllAsync();
            return jobs.Select(j => _mapper.Map<JobListDto>(j));
        }

        public async Task DeleteJobAsync(Guid jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job != null)
            {
                await _jobRepository.DeleteAsync(job);
            }
        }
    }
}
