using ApplicationEntity = InternHub.Domain.Entities.Application; 
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

        [Fact]
        public async Task ApplyToJobAsync_ShouldCallApply_WhenCandidateExists()
        {
            var jobId = Guid.NewGuid();
            var candidateUserId = Guid.NewGuid();
            var candidate = new Candidate { Id = Guid.NewGuid() };
            _candidateRepoMock.Setup(r => r.GetByUserIdAsync(candidateUserId)).ReturnsAsync(candidate);

            var dto = new JobApplicationCreateDto { Message = "Hello" };

            await _service.ApplyToJobAsync(jobId, candidateUserId, dto);

            _appRepoMock.Verify(r => r.ApplyAsync(It.Is<ApplicationEntity>(a =>
                a.CandidateId == candidate.Id &&
                a.JobId == jobId &&
                a.Message == dto.Message &&
                a.Status == ApplicationStatus.Pending
            )), Times.Once);
        }
    }
}
