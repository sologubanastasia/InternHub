using InternHub.Application.Services.Team;
using InternHub.Application.DTO.Team;
using InternHub.Domain.Entities;
using InternHub.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; 
using System.Linq.Expressions;
using System.Threading;
using System.Linq;
using AutoMapper;
using Xunit;
using Moq;

namespace InternHub.Application.Tests.Services.Team
{
    public class TeamServiceTests
    {
        private readonly Mock<ITeamRequestRepository> _mockTeamRequestRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IProjectMemberRepository> _mockProjectMemberRepository;
        private readonly Mock<ICandidateRepository> _mockCandidateRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly TeamService _teamService;

        private readonly Guid _projectId = Guid.NewGuid();
        private readonly Guid _candidateId = Guid.NewGuid();
        private readonly Guid _ownerId = Guid.NewGuid();
        private readonly Guid _requestId = Guid.NewGuid();

        public TeamServiceTests()
        {
            _mockTeamRequestRepository = new Mock<ITeamRequestRepository>();
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockProjectMemberRepository = new Mock<IProjectMemberRepository>();
            _mockCandidateRepository = new Mock<ICandidateRepository>();
            _mockMapper = new Mock<IMapper>();

            _teamService = new TeamService(
                _mockTeamRequestRepository.Object,
                _mockProjectRepository.Object,
                _mockProjectMemberRepository.Object,
                _mockCandidateRepository.Object,
                _mockMapper.Object);
        }

        [Fact]
        public async Task GetTeamProjectsAsync_ShouldReturnMappedProjects()
        {
            var projects = new List<Project> 
            { 
                new Project { Id = _projectId, Name = "Project A" },
                new Project { Id = Guid.NewGuid(), Name = "Project B" }
            };
            var projectsDto = new List<TeamProjectDto> 
            { 
                new TeamProjectDto { Id = _projectId, Name = "Project A" },
                new TeamProjectDto { Id = Guid.NewGuid(), Name = "Project B" }
            };

            var queryableProjects = projects.AsQueryable();
            var mockQueryable = new Mock<IQueryable<Project>>();
            mockQueryable.As<IAsyncEnumerable<Project>>()
                         .Setup(x => x.GetAsyncEnumerator(default))
                         .Returns(new TestAsyncEnumerator<Project>(queryableProjects.GetEnumerator()));
            mockQueryable.As<IQueryable<Project>>()
                         .Setup(x => x.Provider)
                         .Returns(new TestAsyncQueryProvider<Project>(queryableProjects.Provider));
            mockQueryable.As<IQueryable<Project>>()
                         .Setup(x => x.Expression)
                         .Returns(queryableProjects.Expression);
            mockQueryable.As<IQueryable<Project>>()
                         .Setup(x => x.ElementType)
                         .Returns(queryableProjects.ElementType);

            _mockProjectRepository.Setup(r => r.GetActiveProjectsQuery())
                                 .Returns(mockQueryable.Object);
            
            _mockMapper.Setup(m => m.Map<List<TeamProjectDto>>(It.IsAny<List<Project>>()))
                       .Returns(projectsDto);

            var result = await _teamService.GetTeamProjectsAsync();

            Assert.Equal(projectsDto.Count, result.Count);
            _mockProjectRepository.Verify(r => r.GetActiveProjectsQuery(), Times.Once);
            _mockMapper.Verify(m => m.Map<List<TeamProjectDto>>(It.IsAny<List<Project>>()), Times.Once);
        }
        
        private class TestAsyncQueryProvider<T> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;
            public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;
            public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<T>(expression);
            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
            public object? Execute(Expression expression) => _inner.Execute(expression);
            public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression)!;
            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                var expectedResultType = typeof(TResult).GetGenericArguments().First();
                var executionResult = typeof(IQueryProvider)
                    .GetMethods()
                    .Single(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod)
                    .MakeGenericMethod(expectedResultType)
                    .Invoke(_inner, new[] { expression });
                
                return (TResult)Task.FromResult(executionResult);
            }
        }
        
        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }
        }

        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return new ValueTask(Task.CompletedTask);
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }
        }

        [Fact]
        public async Task ApplyToProjectAsync_ShouldApplySuccessfully()
        {
            var applyDto = new ApplyToProjectDto { Message = "Interested" };
            var project = new Project { Id = _projectId, Name = "Test Project" };
            var newRequest = new TeamRequest { Id = _requestId, Message = "Interested" };

            _mockProjectRepository.Setup(r => r.GetByIdAsync(_projectId))
                                 .ReturnsAsync(project);
            _mockProjectMemberRepository.Setup(r => r.IsMemberInProjectAsync(_projectId, _candidateId))
                                         .ReturnsAsync(false);
            _mockTeamRequestRepository.Setup(r => r.HasCandidateApliedAsync(_projectId, _candidateId))
                                     .ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<TeamRequest>(applyDto))
                       .Returns(newRequest);
            
            await _teamService.ApplyToProjectAsync(_projectId, _candidateId, applyDto);

            _mockMapper.Verify(m => m.Map<TeamRequest>(applyDto), Times.Once);
            _mockTeamRequestRepository.Verify(r => r.AddAsync(It.Is<TeamRequest>(tr => 
                tr.ProjectId == _projectId && 
                tr.CandidateId == _candidateId &&
                tr.Status == ApplicationStatus.Pending)), Times.Once);
            _mockTeamRequestRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ApplyToProjectAsync_ShouldThrowKeyNotFoundException_WhenProjectNotFound()
        {
            _mockProjectRepository.Setup(r => r.GetByIdAsync(_projectId))
                                 .ReturnsAsync((Project)null);
            
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _teamService.ApplyToProjectAsync(_projectId, _candidateId, new ApplyToProjectDto()));
        }

        [Fact]
        public async Task ApplyToProjectAsync_ShouldThrowInvalidOperationException_WhenAlreadyMember()
        {
            var project = new Project { Id = _projectId };
            _mockProjectRepository.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync(project);
            _mockProjectMemberRepository.Setup(r => r.IsMemberInProjectAsync(_projectId, _candidateId))
                                         .ReturnsAsync(true);
            
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _teamService.ApplyToProjectAsync(_projectId, _candidateId, new ApplyToProjectDto()));
        }

        [Fact]
        public async Task ApplyToProjectAsync_ShouldThrowInvalidOperationException_WhenPendingRequestExists()
        {
            var project = new Project { Id = _projectId };
            _mockProjectRepository.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync(project);
            _mockProjectMemberRepository.Setup(r => r.IsMemberInProjectAsync(_projectId, _candidateId))
                                         .ReturnsAsync(false);
            _mockTeamRequestRepository.Setup(r => r.HasCandidateApliedAsync(_projectId, _candidateId))
                                     .ReturnsAsync(true);
            
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _teamService.ApplyToProjectAsync(_projectId, _candidateId, new ApplyToProjectDto()));
        }

        [Fact]
        public async Task GetTeamRequestDetailsAsync_ShouldReturnRequests_WhenOwner()
        {
            var requests = new List<TeamRequest> { new TeamRequest { Id = _requestId, ProjectId = _projectId } };
            var requestsDto = new List<TeamRequestDetailsDto> { new TeamRequestDetailsDto { Id = _requestId } };

            _mockProjectRepository.Setup(r => r.IsProjectOwnerAsync(_projectId, _ownerId))
                                 .ReturnsAsync(true);
            _mockTeamRequestRepository.Setup(r => r.GetRequestsTeamIdAsync(_projectId))
                                     .ReturnsAsync(requests);
            _mockMapper.Setup(m => m.Map<IList<TeamRequestDetailsDto>>(requests))
                       .Returns(requestsDto);

            var result = await _teamService.GetTeamRequestDetailsAsync(_ownerId, _projectId);

            Assert.Equal(requestsDto.Count, result.Count);
            _mockProjectRepository.Verify(r => r.IsProjectOwnerAsync(_projectId, _ownerId), Times.Once);
            _mockTeamRequestRepository.Verify(r => r.GetRequestsTeamIdAsync(_projectId), Times.Once);
        }

        [Fact]
        public async Task GetTeamRequestDetailsAsync_ShouldThrowUnauthorizedAccessException_WhenNotOwner()
        {
            _mockProjectRepository.Setup(r => r.IsProjectOwnerAsync(_projectId, _ownerId))
                                 .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _teamService.GetTeamRequestDetailsAsync(_ownerId, _projectId));
            _mockTeamRequestRepository.Verify(r => r.GetRequestsTeamIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ProcessRequestAsync_ShouldThrowUnauthorizedAccessException_WhenNotOwner()
        {
            _mockProjectRepository.Setup(r => r.IsProjectOwnerAsync(_projectId, _ownerId))
                                 .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _teamService.ProcessRequestAsync(_projectId, _requestId, _ownerId, new ProcessRequestDto { Action = ApplicationStatus.Accepted }));
            _mockTeamRequestRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ProcessRequestAsync_ShouldThrowKeyNotFoundException_WhenRequestNotFound()
        {
            _mockProjectRepository.Setup(r => r.IsProjectOwnerAsync(_projectId, _ownerId)).ReturnsAsync(true);
            _mockTeamRequestRepository.Setup(r => r.GetByIdAsync(_requestId)).ReturnsAsync((TeamRequest)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _teamService.ProcessRequestAsync(_projectId, _requestId, _ownerId, new ProcessRequestDto { Action = ApplicationStatus.Accepted }));
        }

        [Fact]
        public async Task ProcessRequestAsync_ShouldThrowKeyNotFoundException_WhenRequestBelongsToAnotherProject()
        {
            var request = new TeamRequest { Id = _requestId, ProjectId = Guid.NewGuid(), Status = ApplicationStatus.Pending, CandidateId = _candidateId };
            _mockProjectRepository.Setup(r => r.IsProjectOwnerAsync(_projectId, _ownerId)).ReturnsAsync(true);
            _mockTeamRequestRepository.Setup(r => r.GetByIdAsync(_requestId)).ReturnsAsync(request);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _teamService.ProcessRequestAsync(_projectId, _requestId, _ownerId, new ProcessRequestDto { Action = ApplicationStatus.Accepted }));
        }

        [Fact]
        public async Task ProcessRequestAsync_ShouldThrowInvalidOperationException_WhenRequestAlreadyProcessed()
        {
            var request = new TeamRequest { Id = _requestId, ProjectId = _projectId, Status = ApplicationStatus.Accepted, CandidateId = _candidateId };
            _mockProjectRepository.Setup(r => r.IsProjectOwnerAsync(_projectId, _ownerId)).ReturnsAsync(true);
            _mockTeamRequestRepository.Setup(r => r.GetByIdAsync(_requestId)).ReturnsAsync(request);

            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _teamService.ProcessRequestAsync(_projectId, _requestId, _ownerId, new ProcessRequestDto { Action = ApplicationStatus.Accepted }));
        }

        [Fact]
        public async Task ProcessRequestAsync_ShouldAcceptRequestAndAddMember()
        {
            var request = new TeamRequest { Id = _requestId, ProjectId = _projectId, Status = ApplicationStatus.Pending, CandidateId = _candidateId };
            var processDto = new ProcessRequestDto { Action = ApplicationStatus.Accepted, Role = "Tester" };

            _mockProjectRepository.Setup(r => r.IsProjectOwnerAsync(_projectId, _ownerId)).ReturnsAsync(true);
            _mockTeamRequestRepository.Setup(r => r.GetByIdAsync(_requestId)).ReturnsAsync(request);
            _mockProjectMemberRepository.Setup(r => r.IsMemberInProjectAsync(_projectId, _candidateId)).ReturnsAsync(false);
            
            await _teamService.ProcessRequestAsync(_projectId, _requestId, _ownerId, processDto);

            Assert.Equal(ApplicationStatus.Accepted, request.Status);
            _mockProjectMemberRepository.Verify(r => r.AddAsync(It.Is<ProjectMember>(pm => 
                pm.ProjectId == _projectId && 
                pm.CandidateId == _candidateId &&
                pm.Role == "Tester")), Times.Once);
            _mockTeamRequestRepository.Verify(r => r.Update(request), Times.Once);
            _mockTeamRequestRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ProcessRequestAsync_ShouldRejectRequestWithoutAddingMember()
        {
            var request = new TeamRequest { Id = _requestId, ProjectId = _projectId, Status = ApplicationStatus.Pending, CandidateId = _candidateId };
            var processDto = new ProcessRequestDto { Action = ApplicationStatus.Rejected, Role = "Ignored" };

            _mockProjectRepository.Setup(r => r.IsProjectOwnerAsync(_projectId, _ownerId)).ReturnsAsync(true);
            _mockTeamRequestRepository.Setup(r => r.GetByIdAsync(_requestId)).ReturnsAsync(request);
            
            await _teamService.ProcessRequestAsync(_projectId, _requestId, _ownerId, processDto);

            Assert.Equal(ApplicationStatus.Rejected, request.Status);
            _mockProjectMemberRepository.Verify(r => r.IsMemberInProjectAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _mockProjectMemberRepository.Verify(r => r.AddAsync(It.IsAny<ProjectMember>()), Times.Never);
            _mockTeamRequestRepository.Verify(r => r.Update(request), Times.Once);
            _mockTeamRequestRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        
        [Fact]
        public async Task ProcessRequestAsync_ShouldAcceptRequestAndUseDefaultRole_WhenRoleIsNull()
        {
            var request = new TeamRequest { Id = _requestId, ProjectId = _projectId, Status = ApplicationStatus.Pending, CandidateId = _candidateId };
            var processDto = new ProcessRequestDto { Action = ApplicationStatus.Accepted, Role = null };

            _mockProjectRepository.Setup(r => r.IsProjectOwnerAsync(_projectId, _ownerId)).ReturnsAsync(true);
            _mockTeamRequestRepository.Setup(r => r.GetByIdAsync(_requestId)).ReturnsAsync(request);
            _mockProjectMemberRepository.Setup(r => r.IsMemberInProjectAsync(_projectId, _candidateId)).ReturnsAsync(false);
            
            await _teamService.ProcessRequestAsync(_projectId, _requestId, _ownerId, processDto);

            Assert.Equal(ApplicationStatus.Accepted, request.Status);
            _mockProjectMemberRepository.Verify(r => r.AddAsync(It.Is<ProjectMember>(pm => 
                pm.Role == "Developer")), Times.Once);
        }

        [Fact]
        public async Task ProcessRequestAsync_ShouldThrowInvalidOperationException_WhenAcceptingExistingMember()
        {
            var request = new TeamRequest { Id = _requestId, ProjectId = _projectId, Status = ApplicationStatus.Pending, CandidateId = _candidateId };
            var processDto = new ProcessRequestDto { Action = ApplicationStatus.Accepted, Role = "Tester" };

            _mockProjectRepository.Setup(r => r.IsProjectOwnerAsync(_projectId, _ownerId)).ReturnsAsync(true);
            _mockTeamRequestRepository.Setup(r => r.GetByIdAsync(_requestId)).ReturnsAsync(request);
            _mockProjectMemberRepository.Setup(r => r.IsMemberInProjectAsync(_projectId, _candidateId)).ReturnsAsync(true); 
            
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _teamService.ProcessRequestAsync(_projectId, _requestId, _ownerId, processDto));

            Assert.Equal(ApplicationStatus.Pending, request.Status);
            _mockTeamRequestRepository.Verify(r => r.Update(It.IsAny<TeamRequest>()), Times.Never);
            _mockTeamRequestRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}