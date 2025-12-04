using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;

namespace InternHub.Application.Services.Company
{
    public interface ICompanyService
    {
        Task<CompanyResponseDto> GetProfileAsync(Guid userId);
        Task UpdateProfileAsync(Guid userId, UpdateCompanyDto dto);
        Task DeleteProfileAsync(Guid userId);

        Task<IEnumerable<CompanyJobListDto>> GetCompanyJobsAsync(Guid userId);
        Task CreateJobAsync(Guid userId, CreateJobDto dto);
        Task<IEnumerable<JobApplicationResponseDto>> GetJobApplicationsAsync(Guid jobId);
        Task DeleteJobAsync(Guid jobId);

        Task UploadDocumentAsync(Guid userId, CompanyDocumentUploadDto dto);
    }
}
