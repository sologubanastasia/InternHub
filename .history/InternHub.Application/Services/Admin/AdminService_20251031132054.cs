using InternHub.Application.DTO.Admin;
using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;
using InternHub.Domain.Entities;
using InternHub.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace InternHub.Application.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJobRepository _jobRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminService(
            IAuthRepository authRepository,
            ICompanyRepository companyRepository,
            IJobRepository jobRepository,
            UserManager<ApplicationUser> userManager)
        {
            _authRepository = authRepository;
            _companyRepository = companyRepository;
            _jobRepository = jobRepository;
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserListDto>> GetAllUsersAsync(int pagedNum = 1, int pageSize = 10)
        {
            var users = await _authRepository.GetAllUsersAsync();

            var pagedUsers = users
                .Skip((pagedNum - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var userDtos = new List<UserListDto>();

            foreach (var user in pagedUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User";

                userDtos.Add(new UserListDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = role,
                    IsActive = user.IsActive
                });
            }

            return userDtos;
        }

        public async Task<IEnumerable<CompanyListDto>> GetAllCompaniesAsync(int pagedNum = 1, int pageSize = 10)
        {
            var companies = await _companyRepository.GetAllAsync();

            var pagedCompanies = companies
                .Skip((pagedNum - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return pagedCompanies.Select(c => new CompanyListDto
            {
                Id = c.Id,
                CompanyName = c.CompanyName,
                Email = c.User?.Email ?? string.Empty,
                Status = c.Status.ToString()
            });
        }

        public async Task ApproveCompanyAsync(Guid id, ApproveCompanyDto dto)
        {
            var company = await _companyRepository.GetByCompanyIdAsync(id);
            if (company == null)
                throw new Exception("Company not found.");

            company.Status = dto.Approve.Value
                ? CompanyStatus.Approved 
                : CompanyStatus.NotApproved;

            await _companyRepository.UpdateAsync(company);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _authRepository.GetUserByIdAsync(id);
            if (user == null)
                throw new Exception("User not found.");

            await _authRepository.DeleteUserAsync(user);
        }

        public async Task<IEnumerable<JobListDto>> GetAllJobsAsync(int pagedNum = 1, int pageSize = 10)
        {
            var jobs = await _jobRepository.GetAllAsync();

            var pagedJobs = jobs
                .Skip((pagedNum - 1) * pageSize)
                .Take(pageSize)
                .ToList(); 

            return jobs.Select(j => new JobListDto
            {
                Id = j.Id,
                Title = j.Title,
                Location = j.Location,
                CompanyName = j.Company?.CompanyName ?? string.Empty,
                IsActive = j.IsActive,
                CreatedDate = j.CreatedDate
            });
        }

        public async Task DeleteJobAsync(Guid id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
                throw new Exception("Job not found.");

            await _jobRepository.DeleteAsync(job);
        }
    }
}
