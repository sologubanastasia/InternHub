using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;

namespace InternHub.Application.Services.Company
{
    public interface ICompanyService
    {
        Task RegisterAsync(RegisterCompanyDto dto);
        Task<bool> LoginAsync(string email, string password);

        Task<CompanyResponseDto> GetProfileAsync(Guid ApplicationUserId);
        Task UpdateProfileAsync(Guid ApplicationUserId, UpdateCompanyDto dto);
        Task DeleteProfileAsync(Guid ApplicationUserId);

        Task CreateJobAsync(Guid companyId, CreateJobDto dto);
        Task<IEnumerable<JobListDto>> GetCompanyJobsAsync(Guid companyId);
        Task<IEnumerable<JobApplicationResponseDto>> GetJobApplicationsAsync(Guid jobId);
        Task DeleteJobAsync(Guid jobId);

        Task UploadDocumentAsync(Guid companyId, CompanyDocumentUploadDto dto);
        Task<IEnumerable<CompanyDocumentUploadDto>> GetDocumentsAsync(Guid companyId);
        Task DeleteDocumentAsync(Guid documentId);
    }
}
