using InternHub.Application.DTO.Project;
using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace InternHub.IntegrationTests.Controllers
{
    [Collection("Sequential")] 
    public class ProjectControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InternHubDbContext _db;
        private readonly IServiceProvider _serviceProvider;
        
        // Фіксовані ID
        private readonly Guid CandidateUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private readonly Guid OwnerCandidateId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        private Guid _testProjectId; 
        private Guid _testMemberCandidateId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        public ProjectControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            var scope = factory.Services.CreateScope();
            _serviceProvider = scope.ServiceProvider;
            _db = _serviceProvider.GetRequiredService<InternHubDbContext>();

            // Встановлюємо OwnerCandidateId як поточного автентифікованого користувача
            _client.DefaultRequestHeaders.Add("X-Test-UserId", OwnerCandidateId.ToString()); 
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Candidate");

            SeedTestProjectDataAsync(_serviceProvider).GetAwaiter().GetResult();
            
            // Ініціалізація змінних
            var testProject = _db.Projects
                                 .Include(p => p.ProjectMembers)
                                 .Single(p => p.CandidateId == OwnerCandidateId); // Шукаємо за власником

            _testProjectId = testProject.Id;
            
            // Встановлюємо ID члена (якщо він є, для подальших тестів)
            var member = testProject.ProjectMembers.FirstOrDefault(m => m.CandidateId != OwnerCandidateId);
            if (member != null)
            {
                _testMemberCandidateId = member.CandidateId;
            }
        }

        private async Task SeedTestProjectDataAsync(IServiceProvider serviceProvider)
        {
            // УВАГА: userManager тут не потрібен, користувачі вже створені у CustomWebApplicationFactory.cs.
            // ВИДАЛЕНО ВСЮ ЛОГІКУ СТВОРЕННЯ КОРИСТУВАЧІВ (1 та 2 блоки)
            
            // 3. Створення тестового проекту
            if (!_db.Projects.Any(p => p.CandidateId == OwnerCandidateId && p.Name == "Test Project Name"))
            {
                // Використовуємо фіксований GUID для проекту
                _testProjectId = Guid.Parse("77777777-7777-7777-7777-777777777777"); 
                
                _db.Projects.Add(new Project
                {
                    Id = _testProjectId,
                    Name = "Test Project Name",
                    Description = "Initial description",
                    CandidateId = OwnerCandidateId, 
                    ProjectMembers = new List<ProjectMember>
                    {
                        new ProjectMember 
                        { 
                            ProjectId = _testProjectId, 
                            CandidateId = _testMemberCandidateId, // Використовуємо існуючого 666...6666
                            Role = "Developer"
                        }
                    }
                });
            }

            await _db.SaveChangesAsync();
        }

        // --- GET /api/projects/my ---
        [Fact]
        public async Task GetProjects_ReturnsOnlyOwnersProjects()
        {
            // ... (тест залишено без змін) ...
            var response = await _client.GetAsync("/api/projects/my");
            response.EnsureSuccessStatusCode();

            var projects = await response.Content.ReadFromJsonAsync<List<ProjectListItemDto>>();
            
            Assert.NotNull(projects);
            Assert.NotEmpty(projects);
            Assert.Contains(projects, p => p.Name == "Test Project Name"); 
        }

        // --- POST /api/projects ---
        [Fact]
        public async Task CreateProject_ReturnsCreated_AndProjectExistsInDb()
        {
            // ... (тест залишено без змін) ...
            var createDto = new CreateProjectDto 
            { 
                Name = "New Test Project", 
                Description = "Description for new project" 
            };
            
            var response = await _client.PostAsJsonAsync("/api/projects", createDto);
            
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var createdProjectDto = await response.Content.ReadFromJsonAsync<ProjectDetailsDto>();
            Assert.NotNull(createdProjectDto);
            Guid projectGuid = createdProjectDto.Id;
            Assert.NotEqual(Guid.Empty, projectGuid);

            var createdProject = await _db.Projects.FindAsync(projectGuid);
            Assert.NotNull(createdProject);
            Assert.Equal(OwnerCandidateId, createdProject.CandidateId); 
            Assert.Equal(createDto.Name, createdProject.Name); 
        }

        // --- GET /api/projects/{projectId} ---
        [Fact]
        public async Task GetProjectById_ReturnsOk_WhenOwnerAccessing()
        {
            // ... (тест залишено без змін) ...
            var response = await _client.GetAsync($"/api/projects/{_testProjectId}");
            response.EnsureSuccessStatusCode(); 

            var project = await response.Content.ReadFromJsonAsync<ProjectDetailsDto>();
            Assert.NotNull(project);
            Assert.Equal(_testProjectId, project.Id);
        }

        // --- PUT /api/projects/{projectId} ---
        [Fact]
        public async Task UpdateProject_ReturnsNoContent_WhenOwnerAccessing()
        {
            // ... (тест залишено без змін) ...
            var updateDto = new UpdateProjectDto
            {
                Name = "Updated Name",
                Description = "Fully updated description"
            };

            var response = await _client.PutAsJsonAsync($"/api/projects/{_testProjectId}", updateDto);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); 

            var entityEntry = _db.Entry(await _db.Projects.FindAsync(_testProjectId)); 
            entityEntry.Reload(); 
            var updatedProject = entityEntry.Entity;
            
            Assert.Equal(updateDto.Name, updatedProject.Name); 
            Assert.Equal(updateDto.Description, updatedProject.Description); 
        }

        [Fact]
        public async Task UpdateProject_ReturnsForbidden_WhenNotOwner()
        {
            // ... (тест залишено без змін) ...
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", CandidateUserId.ToString());

            var updateDto = new UpdateProjectDto 
            {
                Name = "Forbidden Update"
            };

            var response = await _client.PutAsJsonAsync($"/api/projects/{_testProjectId}", updateDto);
            
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        
        // --- POST /api/projects/{projectId}/members ---
        [Fact]
        public async Task AddMember_ReturnsNoContent_AndMemberIsAddedToDb()
        {
            var newCandidateId = Guid.Parse("77777777-7777-7777-7777-777777777777");
            
            // ВИПРАВЛЕНО: Створюємо тільки нового користувача-члена, оскільки він не існує у фабриці
            var userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                Id = newCandidateId,
                UserName = $"newmember_{newCandidateId}@test.com",
                Email = $"newmember_{newCandidateId}@test.com",
                EmailConfirmed = true
            };
            
            if (await userManager.FindByEmailAsync(user.Email) == null)
            {
                await userManager.CreateAsync(user, "Password123!");
                await userManager.AddToRoleAsync(user, RoleConstants.Candidate);
                await _db.Candidates.AddAsync(new Candidate { UserId = newCandidateId, Email = user.Email, Id = newCandidateId });
                await _db.SaveChangesAsync(); // Зберігаємо нового користувача/кандидата
            }
            // Кінець блоку створення нового користувача

            var dto = new AddMemberDto { 
                CandidateId = newCandidateId,
                Role = "Frontend"
            };

            var response = await _client.PostAsJsonAsync($"/api/projects/{_testProjectId}/members", dto);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); 

            var isMemberAdded = _db.ProjectMembers
                .Any(pm => pm.ProjectId == _testProjectId && pm.CandidateId == newCandidateId);
            
            Assert.True(isMemberAdded);
        }
        
        // --- DELETE /api/projects/{projectId}/members/{memberId} ---
        [Fact]
        public async Task RemoveMember_ReturnsNoContent_AndRemovesFromDb()
        {
            // ... (тест залишено без змін) ...
            var response = await _client.DeleteAsync($"/api/projects/{_testProjectId}/members/{_testMemberCandidateId}");
            
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); 

            var isMemberStillPresent = _db.ProjectMembers
                .Any(pm => pm.ProjectId == _testProjectId && pm.CandidateId == _testMemberCandidateId);
            
            Assert.False(isMemberStillPresent);
        }
        
        [Fact]
        public async Task RemoveMember_ReturnsForbidden_WhenNotOwner()
        {
            // ... (тест залишено без змін) ...
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", CandidateUserId.ToString());

            var response = await _client.DeleteAsync($"/api/projects/{_testProjectId}/members/{_testMemberCandidateId}");
            
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}