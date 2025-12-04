using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InternHub.Application.DTO.Project;
using InternHub.Application.Services.Project;
using InternHub.Domain.Entities;
using InternHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ProjectEntities = InternHub.Domain.Entities.Project;
using ProjectMember = InternHub.Domain.Entities.ProjectMember;
using ProjectTechnology = InternHub.Domain.Entities.ProjectTechnology;

namespace InternHub.Application.Tests.Services
{
    public class ProjectServiceTests
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
        private readonly Mock<IProjectTechnologyRepository> _projectTechnologyRepositoryMock;
        private readonly Mock<ITechnologyRepository> _technologyRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ProjectService _service;

        // Тестові дані
        private readonly Guid _ownerId = Guid.NewGuid();
        private readonly Guid _memberId = Guid.NewGuid();
        private readonly Guid _otherCandidateId = Guid.NewGuid();
        private readonly Guid _projectId = Guid.NewGuid();
        private readonly Guid _techId1 = Guid.NewGuid();
        private readonly Guid _techId2 = Guid.NewGuid();

        public ProjectServiceTests()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
            _projectTechnologyRepositoryMock = new Mock<IProjectTechnologyRepository>();
            _technologyRepositoryMock = new Mock<ITechnologyRepository>();
            _mapperMock = new Mock<IMapper>();

            _service = new ProjectService(
                _projectRepositoryMock.Object,
                _projectMemberRepositoryMock.Object,
                _projectTechnologyRepositoryMock.Object,
                _technologyRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        // --------------------------------------------------------------------------------
        // 1. GetProjectsByOwnerAsync
        // --------------------------------------------------------------------------------

        [Fact]
        public async Task GetProjectsByOwnerAsync_ReturnsProjects_OwnedAndMember()
        {
            // Arrange
            var projects = new List<ProjectEntities>
            {
                new ProjectEntities { Id = _projectId, CandidateId = _ownerId, ProjectMembers = new List<ProjectMember>() },
                new ProjectEntities { Id = Guid.NewGuid(), CandidateId = _otherCandidateId, ProjectMembers = new List<ProjectMember> { new ProjectMember { CandidateId = _ownerId } } }
            };

            // Налаштування, що імітує IQueryable
            _projectRepositoryMock
                .Setup(r => r.GetProjectsQuery(_ownerId))
                .Returns(projects.AsQueryable());

            // Налаштування ToListAsync (потрібно для IQueryable)
            var mockSet = projects.AsQueryable().BuildMockDbSet(); // Вимагає Moq.EntityFrameworkCore або подібної бібліотеки
                                                                   // АБО спростіть, імітуючи ToListAsync, якщо ви тестуєте тільки LINQ-запит:
                                                                   
            // Для простих тестів Moq і LINQ, часто просто повертають список, а не імітують IQueryable.
            // Оскільки LINQ-запит відбувається всередині сервісу, ми повинні бути впевнені, що IQueryable повертається коректно.
            // У цьому випадку ми припускаємо, що Linq ToListAsync працює коректно, імітуючи лише IQueryable, 
            // та імітуємо результат мапінгу:

            var expectedProjects = projects.Select(p => new ProjectListItemDto { Id = p.Id }).ToList();

            // Імітуємо фактичну логіку ToListAsync
            _projectRepositoryMock
                .Setup(r => r.GetProjectsQuery(_ownerId))
                .Returns(projects.AsQueryable());

            // Налаштовуємо маппер, який буде викликаний з відфільтрованим списком
            _mapperMock
                .Setup(m => m.Map<List<ProjectListItemDto>>(It.IsAny<List<ProjectEntities>>()))
                .Returns(expectedProjects);

            // Act
            var result = await _service.GetProjectsByOwnerAsync(_ownerId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.Any(p => p.Id == _projectId));
            _projectRepositoryMock.Verify(r => r.GetProjectsQuery(_ownerId), Times.Once);
            _mapperMock.Verify(m => m.Map<List<ProjectListItemDto>>(
                It.Is<List<ProjectEntities>>(list => 
                    list.Count == 2 && 
                    list.Any(p => p.CandidateId == _ownerId) && // Власний
                    list.Any(p => p.CandidateId == _otherCandidateId) // Член
                )), Times.Once);
        }
        
        // --------------------------------------------------------------------------------
        // 2. CreateProjectAsync
        // --------------------------------------------------------------------------------

        [Fact]
        public async Task CreateProjectAsync_Success_WithTechnologies()
        {
            // Arrange
            var createDto = new CreateProjectDto
            {
                Name = "Test Project",
                Description = "Desc",
                ProjectTechnologies = new List<Guid> { _techId1, _techId2 }
            };
            var newProject = new ProjectEntities { Id = _projectId, CandidateId = _ownerId };
            var ownerMember = new ProjectMember { ProjectId = _projectId, CandidateId = _ownerId, Role = "Owner" };

            _technologyRepositoryMock.Setup(r => r.AllExistAsync(createDto.ProjectTechnologies)).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<ProjectEntities>(createDto)).Returns(newProject);

            // Act
            var resultId = await _service.CreateProjectAsync(_ownerId, createDto);

            // Assert
            Assert.Equal(_projectId, resultId);
            _projectRepositoryMock.Verify(r => r.AddAsync(newProject), Times.Once);
            _projectTechnologyRepositoryMock.Verify(r => r.AddRangeAsync(
                It.Is<IEnumerable<ProjectTechnology>>(list => list.Count() == 2)), Times.Once);
            _projectMemberRepositoryMock.Verify(r => r.AddAsync(
                It.Is<ProjectMember>(pm => pm.CandidateId == _ownerId && pm.Role == "Owner")), Times.Once);
            _projectRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateProjectAsync_ThrowsException_WhenTechnologyDoesNotExist()
        {
            // Arrange
            var createDto = new CreateProjectDto
            {
                Name = "Test Project",
                ProjectTechnologies = new List<Guid> { _techId1 }
            };

            _technologyRepositoryMock.Setup(r => r.AllExistAsync(createDto.ProjectTechnologies)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateProjectAsync(_ownerId, createDto));
            _projectRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ProjectEntities>()), Times.Never);
            _projectRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateProjectAsync_Success_NoTechnologies()
        {
            // Arrange
            var createDto = new CreateProjectDto
            {
                Name = "Test Project",
                ProjectTechnologies = new List<Guid>()
            };
            var newProject = new ProjectEntities { Id = _projectId, CandidateId = _ownerId };

            _mapperMock.Setup(m => m.Map<ProjectEntities>(createDto)).Returns(newProject);

            // Act
            var resultId = await _service.CreateProjectAsync(_ownerId, createDto);

            // Assert
            Assert.Equal(_projectId, resultId);
            _projectRepositoryMock.Verify(r => r.AddAsync(newProject), Times.Once);
            _projectTechnologyRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProjectTechnology>>()), Times.Never);
            _projectMemberRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ProjectMember>()), Times.Once);
            _projectRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        
        // --------------------------------------------------------------------------------
        // 3. GetProjetDetailsAsync
        // --------------------------------------------------------------------------------

        [Fact]
        public async Task GetProjetDetailsAsync_Success_AsOwner()
        {
            // Arrange
            var project = new ProjectEntities { Id = _projectId, CandidateId = _ownerId, ProjectMembers = new List<ProjectMember>() };
            var expectedDto = new ProjectDetailsDto { Id = _projectId };
            
            _projectRepositoryMock.Setup(r => r.GetProjectDetailsByIdAsync(_projectId)).ReturnsAsync(project);
            _mapperMock.Setup(m => m.Map<ProjectDetailsDto>(project)).Returns(expectedDto);

            // Act
            var result = await _service.GetProjetDetailsAsync(_projectId, _ownerId);

            // Assert
            Assert.Equal(_projectId, result.Id);
            _projectRepositoryMock.Verify(r => r.GetProjectDetailsByIdAsync(_projectId), Times.Once);
        }
        
        [Fact]
        public async Task GetProjetDetailsAsync_Success_AsMember()
        {
            // Arrange
            var project = new ProjectEntities 
            { 
                Id = _projectId, 
                CandidateId = _otherCandidateId, // Власник не ми
                ProjectMembers = new List<ProjectMember> { new ProjectMember { CandidateId = _memberId } } 
            };
            var expectedDto = new ProjectDetailsDto { Id = _projectId };
            
            _projectRepositoryMock.Setup(r => r.GetProjectDetailsByIdAsync(_projectId)).ReturnsAsync(project);
            _mapperMock.Setup(m => m.Map<ProjectDetailsDto>(project)).Returns(expectedDto);

            // Act
            var result = await _service.GetProjetDetailsAsync(_projectId, _memberId);

            // Assert
            Assert.Equal(_projectId, result.Id);
            _projectRepositoryMock.Verify(r => r.GetProjectDetailsByIdAsync(_projectId), Times.Once);
        }

        [Fact]
        public async Task GetProjetDetailsAsync_ThrowsNotFound_WhenProjectIsNull()
        {
            // Arrange
            _projectRepositoryMock.Setup(r => r.GetProjectDetailsByIdAsync(_projectId)).ReturnsAsync((ProjectEntities)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetProjetDetailsAsync(_projectId, _ownerId));
        }

        [Fact]
        public async Task GetProjetDetailsAsync_ThrowsUnauthorized_WhenNotOwnerOrMember()
        {
            // Arrange
            var project = new ProjectEntities 
            { 
                Id = _projectId, 
                CandidateId = _otherCandidateId, 
                ProjectMembers = new List<ProjectMember>() 
            };
            
            _projectRepositoryMock.Setup(r => r.GetProjectDetailsByIdAsync(_projectId)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetProjetDetailsAsync(_projectId, _ownerId));
        }
        
        // --------------------------------------------------------------------------------
        // 4. AddMemberAsync
        // --------------------------------------------------------------------------------

        [Fact]
        public async Task AddMemberAsync_Success()
        {
            // Arrange
            var project = new ProjectEntities { Id = _projectId, CandidateId = _ownerId };
            var addMemberDto = new AddMemberDto { CandidateId = _memberId, Role = "Dev" };
            
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync(project);
            _projectMemberRepositoryMock.Setup(r => r.IsMemberInProjectAsync(_projectId, _memberId)).ReturnsAsync(false);
            _projectMemberRepositoryMock.Setup(r => r.AddAsync(It.IsAny<ProjectMember>())).Returns(Task.CompletedTask);

            // Act
            await _service.AddMemberAsync(_projectId, _ownerId, addMemberDto);

            // Assert
            _projectMemberRepositoryMock.Verify(r => r.AddAsync(
                It.Is<ProjectMember>(pm => pm.ProjectId == _projectId && pm.CandidateId == _memberId && pm.Role == "Dev")), Times.Once);
            _projectRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        
        [Fact]
        public async Task AddMemberAsync_ThrowsUnauthorized_WhenNotOwner()
        {
            // Arrange
            var project = new ProjectEntities { Id = _projectId, CandidateId = _otherCandidateId };
            var addMemberDto = new AddMemberDto { CandidateId = _memberId, Role = "Dev" };
            
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.AddMemberAsync(_projectId, _ownerId, addMemberDto));
            _projectMemberRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ProjectMember>()), Times.Never);
        }

        [Fact]
        public async Task AddMemberAsync_ThrowsInvalidOperation_WhenAlreadyMember()
        {
            // Arrange
            var project = new ProjectEntities { Id = _projectId, CandidateId = _ownerId };
            var addMemberDto = new AddMemberDto { CandidateId = _memberId, Role = "Dev" };
            
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync(project);
            _projectMemberRepositoryMock.Setup(r => r.IsMemberInProjectAsync(_projectId, _memberId)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddMemberAsync(_projectId, _ownerId, addMemberDto));
            _projectMemberRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ProjectMember>()), Times.Never);
        }
        
        [Fact]
        public async Task AddMemberAsync_ThrowsInvalidOperation_WhenAddingOwner()
        {
            // Arrange
            var project = new ProjectEntities { Id = _projectId, CandidateId = _ownerId };
            var addMemberDto = new AddMemberDto { CandidateId = _ownerId, Role = "Dev" }; // Додаємо самого власника
            
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync(project);
            _projectMemberRepositoryMock.Setup(r => r.IsMemberInProjectAsync(_projectId, _ownerId)).ReturnsAsync(false); // Власник не є членом у таблиці ProjectMembers

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddMemberAsync(_projectId, _ownerId, addMemberDto));
            _projectMemberRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ProjectMember>()), Times.Never);
        }
        
        // --------------------------------------------------------------------------------
        // 5. RemoveMemberAsync
        // --------------------------------------------------------------------------------

        [Fact]
        public async Task RemoveMemberAsync_Success()
        {
            // Arrange
            var project = new ProjectEntities { Id = _projectId, CandidateId = _ownerId };
            var memberToRemove = new ProjectMember { ProjectId = _projectId, CandidateId = _memberId, Role = "Dev" };
            
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync(project);
            _projectMemberRepositoryMock.Setup(r => r.GetMemberByCandidateAndProjectAsync(_projectId, _memberId)).ReturnsAsync(memberToRemove);
            
            // Act
            await _service.RemoveMemberAsync(_projectId, _ownerId, _memberId);

            // Assert
            _projectMemberRepositoryMock.Verify(r => r.Remove(_projectId, _memberId), Times.Once);
            _projectRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        
        [Fact]
        public async Task RemoveMemberAsync_ThrowsNotFound_WhenProjectIsNull()
        {
            // Arrange
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync((ProjectEntities)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.RemoveMemberAsync(_projectId, _ownerId, _memberId));
        }

        [Fact]
        public async Task RemoveMemberAsync_ThrowsUnauthorized_WhenNotOwner()
        {
            // Arrange
            var project = new ProjectEntities { Id = _projectId, CandidateId = _otherCandidateId };
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.RemoveMemberAsync(_projectId, _ownerId, _memberId));
        }

        [Fact]
        public async Task RemoveMemberAsync_ThrowsNotFound_WhenMemberNotFound()
        {
            // Arrange
            var project = new ProjectEntities { Id = _projectId, CandidateId = _ownerId };
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync(project);
            _projectMemberRepositoryMock.Setup(r => r.GetMemberByCandidateAndProjectAsync(_projectId, _memberId)).ReturnsAsync((ProjectMember)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.RemoveMemberAsync(_projectId, _ownerId, _memberId));
        }
        
        [Fact]
        public async Task RemoveMemberAsync_ThrowsInvalidOperation_WhenRemovingOwner()
        {
            // Arrange
            var project = new ProjectEntities { Id = _projectId, CandidateId = _ownerId };
            var ownerMember = new ProjectMember { ProjectId = _projectId, CandidateId = _ownerId, Role = "Owner" }; // Власник у таблиці ProjectMembers (який є власником у Project.CandidateId)
            
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync(project);
            _projectMemberRepositoryMock.Setup(r => r.GetMemberByCandidateAndProjectAsync(_projectId, _ownerId)).ReturnsAsync(ownerMember);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RemoveMemberAsync(_projectId, _ownerId, _ownerId));
        }

        // --------------------------------------------------------------------------------
        // 6. UpdateProjectAsync
        // --------------------------------------------------------------------------------

        [Fact]
        public async Task UpdateProjectAsync_Success_FullUpdate()
        {
            // Arrange
            var existingTechnologies = new List<ProjectTechnology> { new ProjectTechnology { ProjectId = _projectId, TechnologyId = _techId1 } };
            var project = new ProjectEntities 
            { 
                Id = _projectId, 
                CandidateId = _ownerId, 
                Name = "Old Name", 
                ProjectTechnologies = existingTechnologies 
            };
            var updateDto = new UpdateProjectDto 
            { 
                Name = "New Name", 
                Description = "New Desc", 
                ProjectTechnologies = new List<Guid> { _techId2 } // Змінюємо технологію
            };
            
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync(project);
            _technologyRepositoryMock.Setup(r => r.AllExistAsync(updateDto.ProjectTechnologies)).ReturnsAsync(true);
            
            // Act
            await _service.UpdateProjectAsync(_projectId, _ownerId, updateDto);

            // Assert
            // 1. Оновлення основних полів
            _mapperMock.Verify(m => m.Map(updateDto, project), Times.Once);
            Assert.Equal("New Name", project.Name); 
            
            // 2. Оновлення технологій
            _projectTechnologyRepositoryMock.Verify(r => r.RemoveRange(existingTechnologies), Times.Once); // Видалення старих
            _projectTechnologyRepositoryMock.Verify(r => r.AddRangeAsync(
                It.Is<IEnumerable<ProjectTechnology>>(list => 
                    list.Count() == 1 && 
                    list.First().TechnologyId == _techId2)), Times.Once); // Додавання нових
            
            // 3. Збереження
            _projectRepositoryMock.Verify(r => r.Update(project), Times.Once);
            _projectRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        
        [Fact]
        public async Task UpdateProjectAsync_Success_RemoveAllTechnologies()
        {
            // Arrange
            var existingTechnologies = new List<ProjectTechnology> { new ProjectTechnology { ProjectId = _projectId, TechnologyId = _techId1 } };
            var project = new ProjectEntities 
            { 
                Id = _projectId, 
                CandidateId = _ownerId, 
                Name = "Old Name", 
                ProjectTechnologies = existingTechnologies 
            };
            var updateDto = new UpdateProjectDto 
            { 
                Name = "New Name", 
                ProjectTechnologies = new List<Guid>() // Видаляємо всі технології
            };
            
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync(project);
            
            // Act
            await _service.UpdateProjectAsync(_projectId, _ownerId, updateDto);

            // Assert
            _projectTechnologyRepositoryMock.Verify(r => r.RemoveRange(existingTechnologies), Times.Once); // Видалення старих
            _projectTechnologyRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProjectTechnology>>()), Times.Never); // Немає нових
            _projectRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_ThrowsNotFound_WhenProjectIsNull()
        {
            // Arrange
            var updateDto = new UpdateProjectDto { Name = "New Name" };
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync((ProjectEntities)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateProjectAsync(_projectId, _ownerId, updateDto));
        }

        [Fact]
        public async Task UpdateProjectAsync_ThrowsUnauthorized_WhenNotOwner()
        {
            // Arrange
            var project = new ProjectEntities { Id = _projectId, CandidateId = _otherCandidateId };
            var updateDto = new UpdateProjectDto { Name = "New Name" };
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateProjectAsync(_projectId, _ownerId, updateDto));
        }

        [Fact]
        public async Task UpdateProjectAsync_ThrowsException_WhenTechnologyDoesNotExist()
        {
            // Arrange
            var project = new ProjectEntities { Id = _projectId, CandidateId = _ownerId, ProjectTechnologies = new List<ProjectTechnology>() };
            var updateDto = new UpdateProjectDto 
            { 
                Name = "New Name", 
                ProjectTechnologies = new List<Guid> { _techId1 } 
            };

            _projectRepositoryMock.Setup(r => r.GetByIdAsync(_projectId)).ReturnsAsync(project);
            _technologyRepositoryMock.Setup(r => r.AllExistAsync(updateDto.ProjectTechnologies)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateProjectAsync(_projectId, _ownerId, updateDto));
            _projectRepositoryMock.Verify(r => r.Update(It.IsAny<ProjectEntities>()), Times.Never);
            _projectRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}