using InternHub.Application.DTO.Admin;
using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;

namespace InternHub.Application.In
{
    public interface IAdminService
    {
        Task<IEnumerable<UserListDto>> GetAllUsersAsync();
        Task<IEnumerable<CompanyListDto>> GetAllCompaniesAsync();
        Task ApproveCompanyAsync(Guid id, ApproveCompanyDto dto);
        Task DeleteUserAsync(Guid id);

        Task<IEnumerable<JobListDto>> GetAllJobsAsync();
        Task DeleteJobAsync(Guid id);
    }
}
