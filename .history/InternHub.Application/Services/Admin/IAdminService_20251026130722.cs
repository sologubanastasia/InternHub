using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;
using InternHub.Application.DTO.ApplicationUser;

namespace InternHub.Application.Services.Admin
{
    public interface IAdminService
    {
        Task<IEnumerable<ApplicationUserResponseDto>> GetAllApplicationUsersAsync();
        Task DeleteApplicationUserAsync(Guid ApplicationUserId);

        Task<IEnumerable<CompanyResponseDto>> GetAllCompaniesAsync();
        Task ApproveCompanyAsync(Guid companyId);
        Task DeclineCompanyAsync(Guid companyId);

        Task<IEnumerable<JobListDto>> GetAllJobsAsync();
        Task DeleteJobAsync(Guid jobId);
    }
}
