namespace InternHub.UnitTests.Services
{
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

        [Fact]
        public async Task GetAllUsersAsync_ReturnsUsersWithRoles()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = Guid.NewGuid(), Email = "test1@mail.com", IsActive = true },
                new ApplicationUser { Id = Guid.NewGuid(), Email = "test2@mail.com", IsActive = false }
            };
            _authMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(users);
            _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
                            .ReturnsAsync(new List<string> { "Admin" });

            var result = await _adminService.GetAllUsersAsync();

            Assert.Equal(2, result.Count());
            Assert.All(result, u => Assert.Equal("Admin", u.Role));
        }

        [Fact]
        public async Task ApproveCompanyAsync_SetsStatusApproved_WhenApprovedTrue()
        {
            var company = new Company { Id = Guid.NewGuid(), Status = CompanyStatus.WaitingForAdminApproval };
            _companyMock.Setup(r => r.GetByCompanyIdAsync(company.Id)).ReturnsAsync(company);

            var dto = new ApproveCompanyDto { Approve = true };

            await _adminService.ApproveCompanyAsync(company.Id, dto);

            Assert.Equal(CompanyStatus.Approved, company.Status);
            _companyMock.Verify(r => r.UpdateAsync(company), Times.Once);
        }

        [Fact]
        public async Task ApproveCompanyAsync_Throws_WhenCompanyNotFound()
        {
            _companyMock.Setup(r => r.GetByCompanyIdAsync(It.IsAny<Guid>())).ReturnsAsync((Company?)null);

            await Assert.ThrowsAsync<Exception>(() => _adminService.ApproveCompanyAsync(Guid.NewGuid(), new ApproveCompanyDto()));
        }

        [Fact]
        public async Task DeleteUserAsync_Deletes_WhenUserExists()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@mail.com" };
            _authMock.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

            await _adminService.DeleteUserAsync(user.Id);

            _authMock.Verify(r => r.DeleteUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_Throws_WhenUserNotFound()
        {
            _authMock.Setup(r => r.GetUserByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ApplicationUser?)null);

            await Assert.ThrowsAsync<Exception>(() => _adminService.DeleteUserAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetAllJobsAsync_ReturnsJobs()
        {
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
