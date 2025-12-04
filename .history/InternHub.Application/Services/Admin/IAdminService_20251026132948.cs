using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;
using InternHub.Application.DTO.User;
using InternHub.Application.DTO.Auth;
namespace InternHub.Application.Services.Admin
{
    public interface IAdminService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task DeleteUserAsync(Guid userId);

        Task<IEnumerable<CompanyResponseDto>> GetAllCompaniesAsync();
        Task ApproveCompanyAsync(Guid companyId);
        Task DeclineCompanyAsync(Guid companyId);

        Task<IEnumerable<JobListDto>> GetAllJobsAsync();
        Task DeleteJobAsync(Guid jobId);
    }
}
