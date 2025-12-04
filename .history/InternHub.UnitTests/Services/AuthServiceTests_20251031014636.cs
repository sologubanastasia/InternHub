using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using InternHub.Application.DTO.Auth;
using InternHub.Application.Services.Auth;
using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
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

        // ✅ Асинхронний DbSet мок
        private static Mock<DbSet<T>> CreateAsyncDbSetMock<T>(IEnumerable<T> elements) where T : class
        {
            var queryable = elements.AsQueryable();

            var dbSetMock = new Mock<DbSet<T>>();
            dbSetMock.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return dbSetMock;
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnOk_WhenCredentialsValid()
        {
            var dto = new LoginDto { Email = "user@mail.com", Password = "Pass123!" };
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = dto.Email };

            var usersDbSet = CreateAsyncDbSetMock(new List<ApplicationUser> { user });

            _userManagerMock.Setup(u => u.Users).Returns(usersDbSet.Object);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
            _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Candidate" });

            var result = await _authService.LoginAsync(dto);

            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Candidate", result.Data.Role);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUnauthorized_WhenUserNotFound()
        {
            var dto = new LoginDto { Email = "no@mail.com", Password = "Pass123!" };
            var usersDbSet = CreateAsyncDbSetMock(new List<ApplicationUser>());

            _userManagerMock.Setup(u => u.Users).Returns(usersDbSet.Object);

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

            var usersDbSet = CreateAsyncDbSetMock(new List<ApplicationUser> { user });

            _userManagerMock.Setup(u => u.Users).Returns(usersDbSet.Object);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
            _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Company" });

            var result = await _authService.LoginAsync(dto);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Company not approved yet.", result.Error);
        }

        // 🔹 Допоміжні класи для асинхронного IQueryable
        internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;
            internal TestAsyncQueryProvider(IQueryProvider inner) { _inner = inner; }

            public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
            public object Execute(Expression expression) => _inner.Execute(expression);
            public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression) => new TestAsyncEnumerable<TResult>(expression);
            public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken) 
                => Task.FromResult(Execute<TResult>(expression));
        }

        internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) 
                => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            public TestAsyncEnumerator(IEnumerator<T> inner) { _inner = inner; }

            public T Current => _inner.Current;

            public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
        }
    }
}
