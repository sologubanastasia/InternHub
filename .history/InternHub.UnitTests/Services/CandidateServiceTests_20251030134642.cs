namespace InternHub.UnitTests.Services;
using System;
using System.Threading.Tasks;
using InternHub.Application.DTO.Candidate;
using InternHub.Application.Services.Candidate;
using InternHub.Domain.Entities;
using InternHub.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace InternHub.UnitTests.Services
{
    public class CandidateServiceTests
    {
        private readonly Mock<ICandidateRepository> _candidateRepoMock;
        private readonly Mock<IAuthRepository> _authRepoMock;
        private readonly CandidateService _service;

        public CandidateServiceTests()
        {
            _candidateRepoMock = new Mock<ICandidateRepository>();
            _authRepoMock = new Mock<IAuthRepository>();
            _service = new CandidateService(_candidateRepoMock.Object, _authRepoMock.Object);
        }

        // ============================================
        // GetProfileAsync
        // ============================================
        [Fact]
        public async Task GetProfileAsync_ShouldReturnCandidate_WhenExists()
        {
            var userId = Guid.NewGuid();
            var candidate = new Candidate
            {
                Id = Guid.NewGuid(),
                GitHubUrl = "https://github.com/test",
                Telegram = "@test",
                ResumeUrl = "resume.pdf",
                VideoUrl = "video.mp4",
                User = new ApplicationUser { Email = "user@mail.com" }
            };

            _candidateRepoMock.Setup(r => r.GetByUserIdAsync(userId))
                              .ReturnsAsync(candidate);

            var result = await _service.GetProfileAsync(userId);

            Assert.Equal(candidate.Id, result.Id);
            Assert.Equal(candidate.GitHubUrl, result.GitHubUrl);
            Assert.Equal(candidate.Telegram, result.Telegram);
            Assert.Equal(candidate.ResumeUrl, result.ResumeUrl);
            Assert.Equal(candidate.VideoUrl, result.VideoUrl);
            Assert.Equal(candidate.User.Email, result.Email);
        }

        [Fact]
        public async Task GetProfileAsync_ShouldThrow_WhenCandidateNotFound()
        {
            _candidateRepoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>()))
                              .ReturnsAsync((Candidate?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.GetProfileAsync(Guid.NewGuid()));
        }

        // ============================================
        // UpdateProfileAsync
        // ============================================
        [Fact]
        public async Task UpdateProfileAsync_ShouldUpdateFields()
        {
            var userId = Guid.NewGuid();
            var candidate = new Candidate
            {
                GitHubUrl = "old.github",
                Telegram = "oldTelegram",
                User = new ApplicationUser { Email = "old@mail.com" }
            };

            _candidateRepoMock.Setup(r => r.GetByUserIdAsync(userId))
                              .ReturnsAsync(candidate);

            var dto = new UpdateCandidateDto
            {
                GitHubUrl = "new.github",
                Telegram = "newTelegram",
                Email = "new@mail.com"
            };

            await _service.UpdateProfileAsync(userId, dto);

            Assert.Equal(dto.GitHubUrl, candidate.GitHubUrl);
            Assert.Equal(dto.Telegram, candidate.Telegram);
            Assert.Equal(dto.Email, candidate.User.Email);

            _candidateRepoMock.Verify(r => r.UpdateAsync(candidate), Times.Once);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldThrow_WhenCandidateNotFound()
        {
            _candidateRepoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>()))
                              .ReturnsAsync((Candidate?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.UpdateProfileAsync(Guid.NewGuid(), new UpdateCandidateDto()));
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
            _authRepoMock.Setup(r => r.GetUserByIdAsync(It.IsAny<Guid>()))
                         .ReturnsAsync((ApplicationUser?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.DeleteProfileAsync(Guid.NewGuid()));
        }

        // ============================================
        // UpdateResumeAsync / DeleteResumeAsync
        // ============================================
        [Fact]
        public async Task UpdateResumeAsync_ShouldSetResumeUrl()
        {
            var userId = Guid.NewGuid();
            var candidate = new Candidate();
            _candidateRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(candidate);

            var dto = new UploadResumeDto { ResumeUrl = "resume.pdf" };
            await _service.UpdateResumeAsync(userId, dto);

            Assert.Equal(dto.ResumeUrl, candidate.ResumeUrl);
            _candidateRepoMock.Verify(r => r.UpdateAsync(candidate), Times.Once);
        }

        [Fact]
        public async Task DeleteResumeAsync_ShouldSetResumeUrlNull()
        {
            var userId = Guid.NewGuid();
            var candidate = new Candidate { ResumeUrl = "old.pdf" };
            _candidateRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(candidate);

            await _service.DeleteResumeAsync(userId);

            Assert.Null(candidate.ResumeUrl);
            _candidateRepoMock.Verify(r => r.UpdateAsync(candidate), Times.Once);
        }

        // ============================================
        // UpdateVideoAsync / DeleteVideoAsync
        // ============================================
        [Fact]
        public async Task UpdateVideoAsync_ShouldSetVideoUrl()
        {
            var userId = Guid.NewGuid();
            var candidate = new Candidate();
            _candidateRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(candidate);

            var dto = new UploadVideoDto { VideoUrl = "video.mp4" };
            await _service.UpdateVideoAsync(userId, dto);

            Assert.Equal(dto.VideoUrl, candidate.VideoUrl);
            _candidateRepoMock.Verify(r => r.UpdateAsync(candidate), Times.Once);
        }

        [Fact]
        public async Task DeleteVideoAsync_ShouldSetVideoUrlNull()
        {
            var userId = Guid.NewGuid();
            var candidate = new Candidate { VideoUrl = "old.mp4" };
            _candidateRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(candidate);

            await _service.DeleteVideoAsync(userId);

            Assert.Null(candidate.VideoUrl);
            _candidateRepoMock.Verify(r => r.UpdateAsync(candidate), Times.Once);
        }
    }
}
