using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;
using InternHub.Application.DTO.Auth;
namespace InternHub.Application.Services.Company
{
    public interface ICompanyService
    {
        Task RegisterAsync(RegisterCompanyDto dto);
        Task<bool> LoginAsync(string email, string password);

        Task<CompanyResponseDto> GetProfileAsync(Guid userId);
        Task UpdateProfileAsync(Guid userId, UpdateCompanyDto dto);
        Task DeleteProfileAsync(Guid userId);

        Task CreateJobAsync(Guid companyId, CreateJobDto dto);
        Task<IEnumerable<JobListDto>> GetCompanyJobsAsync(Guid companyId);
        Task<IEnumerable<JobApplicationResponseDto>> GetJobApplicationsAsync(Guid jobId);
        Task DeleteJobAsync(Guid jobId);

        Task UploadDocumentAsync(Guid companyId, CompanyDocumentUploadDto dto);
        Task<IEnumerable<CompanyDocumentUploadDto>> GetDocumentsAsync(Guid companyId);
        Task DeleteDocumentAsync(Guid documentId);
    }
}
