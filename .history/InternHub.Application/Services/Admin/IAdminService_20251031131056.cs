using InternHub.Application.DTO.Admin;
using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;

namespace InternHub.Application.Services.Admin
{
    public interface IAdminService
    {
        Task<IEnumerable<UserListDto>> GetAllUsersAsync(int pagedNum = 1, int pageSize = 10);
        Task<IEnumerable<CompanyListDto>> GetAllCompaniesAsync(int pageNum = 1, int pasgeSize = 10);
        Task ApproveCompanyAsync(Guid id, ApproveCompanyDto dto);
        Task DeleteUserAsync(Guid id);
        Task<IEnumerable<JobListDto>> GetAllJobsAsync(int pageNum = 1, int);
        Task DeleteJobAsync(Guid id);
    }
}
