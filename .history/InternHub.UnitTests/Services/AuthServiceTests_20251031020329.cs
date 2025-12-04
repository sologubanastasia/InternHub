using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using InternHub.Application.DTO.Auth;
using InternHub.Application.Services.Auth;
using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.AspNetCore.Identity;
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

        [Fact]
        public async Task RegisterCandidateAsync_ShouldCreateCandidate_WhenValid()
        {
            var dto = new RegisterCandidateDto { Email = "candidate@mail.com", Password = "Pass123!" };
            var user = new ApplicationUser { Email = dto.Email, Id = Guid.NewGuid() };

            _mapperMock.Setup(m => m.Map<ApplicationUser>(dto)).Returns(user);
            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null);
            _userManagerMock.Setup(u => u.CreateAsync(user, dto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(r => r.RoleExistsAsync("Candidate")).ReturnsAsync(true);
            _userManagerMock.Setup(u => u.AddToRoleAsync(user, "Candidate")).ReturnsAsync(IdentityResult.Success);

            var result = await _authService.RegisterCandidateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal("Candidate", result.Role);
            Assert.Equal(user.Id, result.UserId);
        }

        [Fact]
        public async Task RegisterCandidateAsync_ShouldThrow_WhenUserExists()
        {
            var dto = new RegisterCandidateDto { Email = "candidate@mail.com", Password = "Pass123!" };
            var existingUser = new ApplicationUser { Email = dto.Email };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(existingUser);

            await Assert.ThrowsAsync<Exception>(() => _authService.RegisterCandidateAsync(dto));
        }

        [Fact]
        public async Task RegisterCompanyAsync_ShouldCreateCompany_WhenValid()
        {
            var dto = new RegisterCompanyDto { Email = "company@mail.com", Password = "Pass123!", CompanyName = "TestCo" };
            var user = new ApplicationUser { Email = dto.Email, Id = Guid.NewGuid() };

            _mapperMock.Setup(m => m.Map<ApplicationUser>(dto)).Returns(user);
            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null);
            _userManagerMock.Setup(u => u.CreateAsync(user, dto.Password)).ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(r => r.RoleExistsAsync(RoleConstants.Company)).ReturnsAsync(true);
            _userManagerMock.Setup(u => u.AddToRoleAsync(user, RoleConstants.Company)).ReturnsAsync(IdentityResult.Success);
            _dbContextMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await _authService.RegisterCompanyAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal(RoleConstants.Company, result.Role);
            Assert.Equal(user.Id, result.UserId);
        }

       
    }
}
