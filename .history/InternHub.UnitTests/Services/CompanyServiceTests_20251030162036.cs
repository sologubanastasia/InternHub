using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;
using InternHub.Application.Services.Company;
using InternHub.Domain.Entities;
using InternHub.Infrastructure.Repositories;
using ApplicationEntity = InternHub.Domain.Entities.Application;
using Moq;
using Xunit;

namespace InternHub.UnitTests.Services
{
    public class CompanyServiceTests
    {
        private readonly Mock<ICompanyRepository> _companyRepoMock;
        private readonly Mock<IJobRepository> _jobRepoMock;
        private readonly Mock<ICompanyDocumentRepository> _docRepoMock;
        private readonly Mock<IAuthRepository> _authRepoMock;
        private readonly CompanyService _service;

        public CompanyServiceTests()
        {
            _companyRepoMock = new Mock<ICompanyRepository>();
            _jobRepoMock = new Mock<IJobRepository>();
            _docRepoMock = new Mock<ICompanyDocumentRepository>();
            _authRepoMock = new Mock<IAuthRepository>();

            _service = new CompanyService(
                _companyRepoMock.Object,
                _jobRepoMock.Object,
                _docRepoMock.Object,
                _authRepoMock.Object
            );
        }

        // ============================================
        // GetProfileAsync
        // ============================================
        [Fact]
        public async Task GetProfileAsync_ShouldReturnCompany_WhenExists()
        {
            var userId = Guid.NewGuid();
            var company = new Company
            {
                Id = Guid.NewGuid(),
                CompanyName = "TestCo",
                Description = "Desc",
                Website = "test.com",
                Status = CompanyStatus.Active,
                User = new ApplicationUser { Email = "email@test.com" }
            };

            _companyRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(company);

            var result = await _service.GetProfileAsync(userId);

            Assert.Equal(company.Id, result.Id);
            Assert.Equal(company.CompanyName, result.CompanyName);
            Assert.Equal(company.Description, result.Description);
            Assert.Equal(company.Website, result.Website);
            Assert.Equal(company.Status.ToString(), result.Status);
            Assert.Equal(company.User.Email, result.Email);
        }

        [Fact]
        public async Task GetProfileAsync_ShouldThrow_WhenCompanyNotFound()
        {
            _companyRepoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>()))
                            .ReturnsAsync((Company?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.GetProfileAsync(Guid.NewGuid()));
        }

        // ============================================
        // UpdateProfileAsync
        // ============================================
        [Fact]
        public async Task UpdateProfileAsync_ShouldUpdateCompany()
        {
            var userId = Guid.NewGuid();
            var company = new Company
            {
                User = new ApplicationUser()
            };
            _companyRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(company);

            var dto = new UpdateCompanyDto
            {
                CompanyName = "NewCo",
                Description = "NewDesc",
                Website = "new.com",
                Email = "new@mail.com"
            };

            await _service.UpdateProfileAsync(userId, dto);

            Assert.Equal(dto.CompanyName, company.CompanyName);
            Assert.Equal(dto.Description, company.Description);
            Assert.Equal(dto.Website, company.Website);
            Assert.Equal(dto.Email, company.User.Email);

            _companyRepoMock.Verify(r => r.UpdateAsync(company), Times.Once);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldThrow_WhenCompanyNotFound()
        {
            _companyRepoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((Company?)null);
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateProfileAsync(Guid.NewGuid(), new UpdateCompanyDto()));
        }

        // ============================================
        // DeleteProfileAsync
        // ============================================
        [Fact]
        public async Task DeleteProfileAsync_ShouldCallDeleteUser()
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser();
            _authRepoMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);

            await _service.DeleteProfileAsync(userId);

            _authRepoMock.Verify(r => r.DeleteUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteProfileAsync_ShouldThrow_WhenUserNotFound()
        {
            _authRepoMock.Setup(r => r.GetUserByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ApplicationUser?)null);
            await Assert.ThrowsAsync<Exception>(() => _service.DeleteProfileAsync(Guid.NewGuid()));
        }

        // ============================================
        // GetCompanyJobsAsync
        // ============================================
        [Fact]
        public async Task GetCompanyJobsAsync_ShouldReturnJobs()
        {
            var userId = Guid.NewGuid();
            var company = new Company { Id = Guid.NewGuid(), CompanyName = "Co" };
            var jobs = new List<Job>
            {
                new Job { Id = Guid.NewGuid(), Title = "Job1", CompanyId = company.Id, Applications = new List<Application>() },
                new Job { Id = Guid.NewGuid(), Title = "Job2", CompanyId = company.Id, Applications = new List<Application> { new Application() } }
            };

            _companyRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(company);
            _jobRepoMock.Setup(r => r.GetByCompanyIdAsync(company.Id)).ReturnsAsync(jobs);

            var result = await _service.GetCompanyJobsAsync(userId);

            Assert.Equal(2, result.Count());
            Assert.Contains(result, j => j.Title == "Job1");
            Assert.Contains(result, j => j.Title == "Job2");
        }

        // ============================================
        // CreateJobAsync
        // ============================================
        [Fact]
        public async Task CreateJobAsync_ShouldAddJob()
        {
            var userId = Guid.NewGuid();
            var company = new Company { Id = Guid.NewGuid() };
            _companyRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(company);

            var dto = new CreateJobDto { Title = "Job", Location = "Loc", Requirements = "Req" };
            await _service.CreateJobAsync(userId, dto);

            _jobRepoMock.Verify(r => r.AddAsync(It.Is<Job>(j =>
                j.Title == dto.Title &&
                j.Location == dto.Location &&
                j.Requirements == dto.Requirements &&
                j.CompanyId == company.Id
            )), Times.Once);
        }

        [Fact]
        public async Task CreateJobAsync_ShouldThrow_WhenCompanyNotFound()
        {
            _companyRepoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((Company?)null);
            await Assert.ThrowsAsync<Exception>(() => _service.CreateJobAsync(Guid.NewGuid(), new CreateJobDto()));
        }

        // ============================================
        // GetJobApplicationsAsync
        // ============================================
       [Fact]
        public async Task GetJobApplicationsAsync_ShouldReturnApplications()
        {
            var jobId = Guid.NewGuid();
            var job = new Job
            {
                Title = "Job1",
                Company = new Company { CompanyName = "Co" },
                Applications = new List<ApplicationEntity>
                {
                    new ApplicationEntity { Id = Guid.NewGuid(), AppliedDate = DateTime.UtcNow, Status = ApplicationStatus.Pending }
                }
            };
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync(job);

            var result = await _service.GetJobApplicationsAsync(jobId);

            Assert.Single(result);
            Assert.Equal("Job1", result.First().JobTitle);
            Assert.Equal("Co", result.First().CompanyName);
            Assert.Equal(ApplicationStatus.Pending.ToString(), result.First().Status);
        }

        // ============================================
        // DeleteJobAsync
        // ============================================
        [Fact]
        public async Task DeleteJobAsync_ShouldCallDelete()
        {
            var jobId = Guid.NewGuid();
            var job = new Job();
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync(job);

            await _service.DeleteJobAsync(jobId);

            _jobRepoMock.Verify(r => r.DeleteAsync(job), Times.Once);
        }

        [Fact]
        public async Task DeleteJobAsync_ShouldThrow_WhenJobNotFound()
        {
            _jobRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Job?)null);
            await Assert.ThrowsAsync<Exception>(() => _service.DeleteJobAsync(Guid.NewGuid()));
        }

        // ============================================
        // UploadDocumentAsync
        // ============================================
        [Fact]
        public async Task UploadDocumentAsync_ShouldAddDocument()
        {
            var userId = Guid.NewGuid();
            var company = new Company { Id = Guid.NewGuid() };
            _companyRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(company);

            var dto = new CompanyDocumentUploadDto { FileName = "doc.pdf", FileUrl = "url.pdf" };
            await _service.UploadDocumentAsync(userId, dto);

            _docRepoMock.Verify(r => r.AddAsync(It.Is<CompanyDocument>(d =>
                d.FileName == dto.FileName &&
                d.FileUrl == dto.FileUrl &&
                d.CompanyId == company.Id
            )), Times.Once);
        }
    }
}
