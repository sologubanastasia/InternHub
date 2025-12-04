using AutoMapper;
using InternHub.Domain;
using InternHub.Infrastructure.Repositories;
using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;

namespace InternHub.Application.Services.Company
{
    public class CompanyService : ICompanyService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ICompanyDocumentRepository _documentRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IMapper _mapper;

        public CompanyService(
            IAuthRepository authRepository,
            ICompanyRepository companyRepository,
            IJobRepository jobRepository,
            ICompanyDocumentRepository documentRepository,
            IApplicationRepository applicationRepository,
            IMapper mapper)
        {
            _authRepository = authRepository;
            _companyRepository = companyRepository;
            _jobRepository = jobRepository;
            _documentRepository = documentRepository;
            _applicationRepository = applicationRepository;
            _mapper = mapper;
        }

        public async Task RegisterAsync(RegisterCompanyDto dto)
        {
            var ApplicationUser = _mapper.Map<ApplicationUser>(dto);
            ApplicationUser.Company = new Company { Status = CompanyStatus.WaitingForAdminApproval };
            await _authRepository.RegisterApplicationUserAsync(ApplicationUser);
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var ApplicationUser = await _authRepository.GetApplicationUserByEmailAsync(email);
            if (ApplicationUser?.Company?.Status != CompanyStatus.Approved) return false;
            return await _authRepository.ValidateApplicationUserCredentialAsync(email, password);
        }

        public async Task<CompanyResponseDto> GetProfileAsync(Guid ApplicationUserId)
        {
            var company = await _companyRepository.GetByApplicationUserIdAsync(ApplicationUserId);
            return _mapper.Map<CompanyResponseDto>(company);
        }

        public async Task UpdateProfileAsync(Guid ApplicationUserId, UpdateCompanyDto dto)
        {
            var company = await _companyRepository.GetByApplicationUserIdAsync(ApplicationUserId);
            _mapper.Map(dto, company);
            await _companyRepository.UpdateAsync(company);
        }

        public async Task DeleteProfileAsync(Guid ApplicationUserId)
        {
            var company = await _companyRepository.GetByApplicationUserIdAsync(ApplicationUserId);
            await _companyRepository.DeleteAsync(company);
        }

        public async Task CreateJobAsync(Guid companyId, CreateJobDto dto)
        {
            var job = _mapper.Map<Job>(dto);
            job.CompanyId = companyId;
            await _jobRepository.AddAsync(job);
        }

        public async Task<IEnumerable<JobListDto>> GetCompanyJobsAsync(Guid companyId)
        {
            var jobs = await _jobRepository.GetByCompanyIdAsync(companyId);
            return jobs.Select(j => _mapper.Map<JobListDto>(j));
        }

        public async Task<IEnumerable<JobApplicationResponseDto>> GetJobApplicationsAsync(Guid jobId)
        {
            var applications = await _applicationRepository.GetApplicationsByJobIdAsync(jobId);
            return applications.Select(a => _mapper.Map<JobApplicationResponseDto>(a));
        }

        public async Task DeleteJobAsync(Guid jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            await _jobRepository.DeleteAsync(job);
        }

        public async Task UploadDocumentAsync(Guid companyId, CompanyDocumentUploadDto dto)
        {
            var document = _mapper.Map<CompanyDocument>(dto);
            document.CompanyId = companyId;
            await _documentRepository.AddAsync(document);
        }

        public async Task<IEnumerable<CompanyDocumentUploadDto>> GetDocumentsAsync(Guid companyId)
        {
            var documents = await _documentRepository.GetByCompanyIdAsync(companyId);
            return documents.Select(d => _mapper.Map<CompanyDocumentUploadDto>(d));
        }

        public async Task DeleteDocumentAsync(Guid documentId)
        {
            var documents = await _documentRepository.GetByCompanyIdAsync(Guid.Empty); // TBD: replace with proper lookup
            var doc = documents.FirstOrDefault(d => d.Id == documentId);
            if (doc != null) await _documentRepository.DeleteAsync(_mapper.Map<CompanyDocument>(doc));
        }
    }
}
