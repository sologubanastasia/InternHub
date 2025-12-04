using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InternHub.Application.DTO.Auth;
using InternHub.Application.Services.Auth;
using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InternHub.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<InternHubDbContext> _dbContextMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole<Guid>>> _roleManagerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _dbContextMock = new Mock<InternHubDbContext>();
            _mapperMock = new Mock<IMapper>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null
            );

            var roleStoreMock = new Mock<IRoleStore<IdentityRole<Guid>>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole<Guid>>>(
                roleStoreMock.Object, null, null, null, null
            );

            _authService = new AuthService(
                _dbContextMock.Object,
                _mapperMock.Object,
                _userManagerMock.Object,
                _roleManagerMock.Object
            );
        }

        // ============================================
        // RegisterCandidateAsync Tests
        // ============================================
        [Fact]
        public async Task RegisterCandidateAsync_ShouldRegister_WhenDataValid()
        {
            // Arrange
            var dto = new RegisterCandidateDto
            {
                Email = "user@mail.com",
                Password = "Pass123!",
                Name = "John",
                Surname = "Doe"
            };

            var user = new ApplicationUser { Email = dto.Email };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email))
                            .ReturnsAsync((ApplicationUser?)null);
            _mapperMock.Setup(m => m.Map<ApplicationUser>(dto))
                       .Returns(user);
            _userManagerMock.Setup(u => u.CreateAsync(user, dto.Password))
                            .ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(r => r.RoleExistsAsync("Candidate"))
                            .ReturnsAsync(true);
            _userManagerMock.Setup(u => u.AddToRoleAsync(user, "Candidate"))
                            .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterCandidateAsync(dto);

            // Assert
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal("Candidate", result.Role);
            Assert.False(string.IsNullOrEmpty(result.Token));
            _userManagerMock.Verify(u => u.CreateAsync(user, dto.Password), Times.Once);
        }

        [Fact]
        public async Task RegisterCandidateAsync_ShouldThrow_WhenEmailExists()
        {
            var dto = new RegisterCandidateDto { Email = "exists@mail.com" };
            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email))
                            .ReturnsAsync(new ApplicationUser());

            await Assert.ThrowsAsync<Exception>(() => _authService.RegisterCandidateAsync(dto));
        }

        // ============================================
        // RegisterCompanyAsync Tests
        // ============================================
        [Fact]
        public async Task RegisterCompanyAsync_ShouldCreateCompanyUser_WhenValid()
        {
            // Arrange
            var dto = new RegisterCompanyDto
            {
                Email = "company@mail.com",
                Password = "Pass123!",
                Name = "Alice",
                CompanyName = "TechCorp"
            };

            var user = new ApplicationUser { Email = dto.Email };

            _mapperMock.Setup(m => m.Map<ApplicationUser>(dto)).Returns(user);
            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(u => u.CreateAsync(user, dto.Password)).ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(r => r.RoleExistsAsync(RoleConstants.Company)).ReturnsAsync(true);
            _userManagerMock.Setup(u => u.AddToRoleAsync(user, RoleConstants.Company))
                            .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterCompanyAsync(dto);

            // Assert
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal(RoleConstants.Company, result.Role);
            Assert.NotNull(user.Company);
            Assert.Equal(CompanyStatus.WaitingForAdminApproval, user.Company.Status);
        }

        [Fact]
        public async Task RegisterCompanyAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            var dto = new RegisterCompanyDto { Email = "exists@mail.com" };
            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email))
                            .ReturnsAsync(new ApplicationUser());

            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterCompanyAsync(dto));
        }

        [Fact]
        public async Task RegisterCompanyAsync_ShouldThrow_WhenPasswordMissing()
        {
            var dto = new RegisterCompanyDto { Email = "test@mail.com", Password = "", CompanyName = "Tech" };

            await Assert.ThrowsAsync<ArgumentException>(() => _authService.RegisterCompanyAsync(dto));
        }

        // ============================================
        // LoginAsync Tests
        // ============================================
        [Fact]
        public async Task LoginAsync_ShouldReturnOk_WhenCredentialsValid()
        {
            // Arrange
            var dto = new LoginDto { Email = "user@mail.com", Password = "Pass123!" };
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = dto.Email };

            var users = new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet();

            _userManagerMock.Setup(u => u.Users).Returns(users.Object);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, dto.Password))
                            .ReturnsAsync(true);
            _userManagerMock.Setup(u => u.GetRolesAsync(user))
                            .ReturnsAsync(new List<string> { "Candidate" });

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Candidate", result.Data.Role);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUnauthorized_WhenUserNotFound()
        {
            var dto = new LoginDto { Email = "no@mail.com", Password = "Pass123!" };
            var users = new List<ApplicationUser>().AsQueryable().BuildMockDbSet();
            _userManagerMock.Setup(u => u.Users).Returns(users.Object);

            var result = await _authService.LoginAsync(dto);

            Assert.False(result.Success);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnBadRequest_WhenCompanyNotApproved()
        {
            // Arrange
            var dto = new LoginDto { Email = "comp@mail.com", Password = "123" };
            var user = new ApplicationUser
            {
                Email = dto.Email,
                Company = new Company { Status = CompanyStatus.WaitingForAdminApproval }
            };

            var users = new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet();

            _userManagerMock.Setup(u => u.Users).Returns(users.Object);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
            _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Company" });

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Company not approved yet.", result.Error);
        }
    }
}
