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
using MockQueryable.Moq;
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
        // LoginAsync Tests
        // ============================================
       [Fact]
        public async Task LoginAsync_ShouldReturnOk_WhenCredentialsValid()
        {
            var dto = new LoginDto { Email = "user@mail.com", Password = "Pass123!" };
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = dto.Email };

            // Правильне мокання Users
            var users = new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet();

            _userManagerMock.Setup(u => u.Users).Returns(users.Object);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, dto.Password))
                            .ReturnsAsync(true);
            _userManagerMock.Setup(u => u.GetRolesAsync(user))
                            .ReturnsAsync(new List<string> { "Candidate" });

            var result = await _authService.LoginAsync(dto);

            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Candidate", result.Data.Role);
        }


        [Fact]
        public async Task LoginAsync_ShouldReturnUnauthorized_WhenUserNotFound()
        {
            var dto = new LoginDto { Email = "no@mail.com", Password = "Pass123!" };
            var users = new List<ApplicationUser>().AsQueryable().BuildMock(); // <-- зміна
            _userManagerMock.Setup(u => u.Users).Returns(users.Object);

            var result = await _authService.LoginAsync(dto);

            Assert.False(result.Success);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnBadRequest_WhenCompanyNotApproved()
        {
            var dto = new LoginDto { Email = "comp@mail.com", Password = "123" };
            var user = new ApplicationUser
            {
                Email = dto.Email,
                Company = new Company { Status = CompanyStatus.WaitingForAdminApproval }
            };

            var users = new List<ApplicationUser> { user }.AsQueryable().BuildMock(); // <-- зміна

            _userManagerMock.Setup(u => u.Users).Returns(users.Object);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
            _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Company" });

            var result = await _authService.LoginAsync(dto);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Company not approved yet.", result.Error);
        }
    }
}
