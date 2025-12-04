using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;
using InternHub.Application.DTO.User;
using InternHub.Application.DTO.Auth;
using InternHub.Application.DTO.Admin;


namespace InternHub.Application.Services.Admin
{
    public interface IAdminService
    {
        Task<IEnumerable<UserListDto>> GetAllUsersAsync();
        Task DeleteUserAsync(Guid userId);
        Task<IEnumerable<CompanyListDto>> GetAllCompaniesAsync();
        Task ApproveCompanyAsync(Guid companyId);
        Task DeclineCompanyAsync(Guid companyId);
        Task<IEnumerable<JobListDto>> GetAllJobsAsync();
        Task DeleteJobAsync(Guid jobId);
    }
}
