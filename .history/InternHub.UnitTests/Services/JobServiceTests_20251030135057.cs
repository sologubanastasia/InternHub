using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InternHub.Application.DTO.Job;
using InternHub.Application.Services.Job;
using InternHub.Domain.Entities;
using InternHub.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace InternHub.UnitTests.Services
{
    public class JobServiceTests
    {
        private readonly Mock<IJobRepository> _jobRepoMock;
        private readonly Mock<IApplicationRepository> _appRepoMock;
        private readonly Mock<ICandidateRepository> _candidateRepoMock;
        private readonly JobService _service;

        public JobServiceTests()
        {
            _jobRepoMock = new Mock<IJobRepository>();
            _appRepoMock = new Mock<IApplicationRepository>();
            _candidateRepoMock = new Mock<ICandidateRepository>();

            _service = new JobService(
                _jobRepoMock.Object,
                _appRepoMock.Object,
                _candidateRepoMock.Object
            );
        }

        // ============================================
        // GetAllJobsAsync
        // ============================================
        [Fact]
        public async Task GetAllJobsAsync_ShouldReturnJobList()
        {
            var jobs = new List<Job>
            {
                new Job { Id = Guid.NewGuid(), Title = "Job1", Location = "Loc1", IsActive = true, CreatedDate = DateTime.UtcNow, Company = new Company { CompanyName = "Co1" } },
                new Job { Id = Guid.NewGuid(), Title = "Job2", Location = "Loc2", IsActive = false, CreatedDate = DateTime.UtcNow, Company = new Company { CompanyName = "Co2" } }
            };

            _jobRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(jobs);

            var result = await _service.GetAllJobsAsync();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, j => j.Title == "Job1" && j.CompanyName == "Co1");
            Assert.Contains(result, j => j.Title == "Job2" && j.CompanyName == "Co2");
        }

        // ============================================
        // GetJobByIdAsync
        // ============================================
        [Fact]
        public async Task GetJobByIdAsync_ShouldReturnJobDetail_WhenJobExists()
        {
            var jobId = Guid.NewGuid();
            var job = new Job
            {
                Id = jobId,
                Title = "Job1",
                Requirements = "Req",
                Location = "Loc",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                Company = new Company { CompanyName = "Co1", Description = "Desc" }
            };

            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync(job);

            var result = await _service.GetJobByIdAsync(jobId);

            Assert.Equal(jobId, result.Id);
            Assert.Equal("Job1", result.Title);
            Assert.Equal("Req", result.Requirements);
            Assert.Equal("Loc", result.Location);
            Assert.Equal("Co1", result.CompanyName);
            Assert.Equal("Desc", result.CompanyDescription);
            Assert.Equal(job.IsActive, result.IsActive);
        }

        [Fact]
        public async Task GetJobByIdAsync_ShouldThrow_WhenJobNotFound()
        {
            _jobRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Job?)null);
            await Assert.ThrowsAsync<Exception>(() => _service.GetJobByIdAsync(Guid.NewGuid()));
        }

        // ============================================
        // ApplyToJobAsync
        // ============================================
        [Fact]
        public async Task ApplyToJobAsync_ShouldCallApply_WhenCandidateExists()
        {
            var jobId = Guid.NewGuid();
            var candidateUserId = Guid.NewGuid();
            var candidate = new Candidate { Id = Guid.NewGuid() };
            _candidateRepoMock.Setup(r => r.GetByUserIdAsync(candidateUserId)).ReturnsAsync(candidate);

            var dto = new JobApplicationCreateDto { Message = "Hello" };

            await _service.ApplyToJobAsync(jobId, candidateUserId, dto);

            _appRepoMock.Verify(r => r.ApplyAsync(It.Is<Application>(a =>
                a.CandidateId == candidate.Id &&
                a.JobId == jobId &&
                a.Message == dto.Message &&
                a.Status == ApplicationStatus.Pending
            )), Times.Once);
        }

        [Fact]
        public async Task ApplyToJobAsync_ShouldThrow_WhenCandidateNotFound()
        {
            _candidateRepoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((Candidate?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.ApplyToJobAsync(Guid.NewGuid(), Guid.NewGuid(), new JobApplicationCreateDto()));
        }
    }
}
