using AutoMapper;
using InternHub.Domain;
using InternHub.Infrastructure.Repositories;
using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;
using InternHub.Application.DTO.User
using InternHub.Application.DTO.Auth;
using InternHub.Application.DTO.Admin;

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

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _authRepository.GetAllUsersAsync();
            return users.Select(u => _mapper.Map<UserResponseDto>(u));
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user != null)
            {
                await _authRepository.DeleteUserAsync(user);
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
