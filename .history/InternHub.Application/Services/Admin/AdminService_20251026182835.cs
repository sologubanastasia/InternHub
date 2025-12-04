using InternHub.Application.DTO.Admin;
using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;
using InternHub.Domain;
using InternHub.Infrastructure.Repositories;

namespace InternHub.Application.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJobRepository _jobRepository;

        public AdminService(IAuthRepository authRepository, ICompanyRepository companyRepository, IJobRepository jobRepository)
        {
            _authRepository = authRepository;
            _companyRepository = companyRepository;
            _jobRepository = jobRepository;
        }

        public async Task<IEnumerable<UserListDto>> GetAllUsersAsync()
        {
            var users = await _authRepository.GetAllUsersAsync();
            return users.Select(u => new UserListDto
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role.ToString(),
                IsActive = true
            });
        }

        public async Task<IEnumerable<CompanyListDto>> GetAllCompaniesAsync()
        {
            var companies = await _companyRepository.GetAllAsync();
            return companies.Select(c => new CompanyListDto
            {
                Id = c.Id,
                CompanyName = c.CompanyName,
                Email = c.User.Email,
                Status = c.Status.ToString()
            });
        }

        public async Task ApproveCompanyAsync(Guid id, ApproveCompanyDto dto)
        {
            var company = await _companyRepository.GetByCompanyIdAsync(id);
            if (company == null) throw new Exception("Company not found.");

            company.Status = dto.Approve ? CompanyStatus.Approved : CompanyStatus.NotApproved;
            await _companyRepository.UpdateAsync(company);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _authRepository.GetUserByIdAsync(id);
            if (user == null) throw new Exception("User not found.");
            await _authRepository.DeleteUserAsync(user);
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

        public async Task DeleteJobAsync(Guid id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null) throw new Exception("Job not found.");
            await _jobRepository.DeleteAsync(job);
        }
    }
}
