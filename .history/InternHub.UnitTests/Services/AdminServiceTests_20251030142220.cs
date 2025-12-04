namespace InternHub.UnitTests.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InternHub.Application.DTO.Admin;
using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;
using InternHub.Application.Services.Admin;
using InternHub.Domain.Entities;
using InternHub.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
    public class AdminServiceTests
    {
        private readonly Mock<IAuthRepository> _authMock;
        private readonly Mock<ICompanyRepository> _companyMock;
        private readonly Mock<IJobRepository> _jobMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly AdminService _adminService;

        public AdminServiceTests()
        {
            _authMock = new Mock<IAuthRepository>();
            _companyMock = new Mock<ICompanyRepository>();
            _jobMock = new Mock<IJobRepository>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null
            );

            _adminService = new AdminService(
                _authMock.Object,
                _companyMock.Object,
                _jobMock.Object,
                _userManagerMock.Object
            );
        }

        // ----------------------------
        // GetAllUsersAsync
        // ----------------------------
        [Fact]
        public async Task GetAllUsersAsync_ReturnsUsersWithRoles()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = Guid.NewGuid(), Email = "test1@mail.com", IsActive = true },
                new ApplicationUser { Id = Guid.NewGuid(), Email = "test2@mail.com", IsActive = false }
            };
            _authMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(users);
            _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
                            .ReturnsAsync(new List<string> { "Admin" });

            // Act
            var result = await _adminService.GetAllUsersAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, u => Assert.Equal("Admin", u.Role));
        }

        // ----------------------------
        // GetAllCompaniesAsync
        // ----------------------------
        [Fact]
        public async Task GetAllCompaniesAsync_ReturnsCompanyDtos()
        {
            // Arrange
            var companies = new List<Company>
            {
                new Company { Id = Guid.NewGuid(), CompanyName = "TestCo", Status = CompanyStatus.Pending, User = new ApplicationUser { Email = "company@mail.com" } }
            };
            _companyMock.Setup(r => r.GetAllAsync()).ReturnsAsync(companies);

            // Act
            var result = await _adminService.GetAllCompaniesAsync();

            // Assert
            var company = Assert.Single(result);
            Assert.Equal("TestCo", company.CompanyName);
            Assert.Equal("company@mail.com", company.Email);
            Assert.Equal("Pending", company.Status);
        }

        // ----------------------------
        // ApproveCompanyAsync
        // ----------------------------
        [Fact]
        public async Task ApproveCompanyAsync_SetsStatusApproved_WhenApprovedTrue()
        {
            // Arrange
            var company = new Company { Id = Guid.NewGuid(), Status = CompanyStatus.Pending };
            _companyMock.Setup(r => r.GetByCompanyIdAsync(company.Id)).ReturnsAsync(company);

            var dto = new ApproveCompanyDto { Approve = true };

            // Act
            await _adminService.ApproveCompanyAsync(company.Id, dto);

            // Assert
            Assert.Equal(CompanyStatus.Approved, company.Status);
            _companyMock.Verify(r => r.UpdateAsync(company), Times.Once);
        }

        [Fact]
        public async Task ApproveCompanyAsync_Throws_WhenCompanyNotFound()
        {
            // Arrange
            _companyMock.Setup(r => r.GetByCompanyIdAsync(It.IsAny<Guid>())).ReturnsAsync((Company?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _adminService.ApproveCompanyAsync(Guid.NewGuid(), new ApproveCompanyDto()));
        }

        // ----------------------------
        // DeleteUserAsync
        // ----------------------------
        [Fact]
        public async Task DeleteUserAsync_Deletes_WhenUserExists()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@mail.com" };
            _authMock.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

            // Act
            await _adminService.DeleteUserAsync(user.Id);

            // Assert
            _authMock.Verify(r => r.DeleteUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_Throws_WhenUserNotFound()
        {
            _authMock.Setup(r => r.GetUserByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ApplicationUser?)null);

            await Assert.ThrowsAsync<Exception>(() => _adminService.DeleteUserAsync(Guid.NewGuid()));
        }

        // ----------------------------
        // GetAllJobsAsync
        // ----------------------------
        [Fact]
        public async Task GetAllJobsAsync_ReturnsJobs()
        {
            // Arrange
            var jobs = new List<Job>
            {
                new Job { Id = Guid.NewGuid(), Title = "Intern", Location = "Kyiv", IsActive = true, CreatedDate = DateTime.UtcNow, Company = new Company { CompanyName = "SoftServe" } }
            };
            _jobMock.Setup(r => r.GetAllAsync()).ReturnsAsync(jobs);

            // Act
            var result = await _adminService.GetAllJobsAsync();

            // Assert
            var job = Assert.Single(result);
            Assert.Equal("Intern", job.Title);
            Assert.Equal("SoftServe", job.CompanyName);
        }

        // ----------------------------
        // DeleteJobAsync
        // ----------------------------
        [Fact]
        public async Task DeleteJobAsync_Deletes_WhenJobExists()
        {
            // Arrange
            var job = new Job { Id = Guid.NewGuid(), Title = "Intern" };
            _jobMock.Setup(r => r.GetByIdAsync(job.Id)).ReturnsAsync(job);

            // Act
            await _adminService.DeleteJobAsync(job.Id);

            // Assert
            _jobMock.Verify(r => r.DeleteAsync(job), Times.Once);
        }

        [Fact]
        public async Task DeleteJobAsync_Throws_WhenJobNotFound()
        {
            _jobMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Job?)null);

            await Assert.ThrowsAsync<Exception>(() => _adminService.DeleteJobAsync(Guid.NewGuid()));
        }
    }
}
