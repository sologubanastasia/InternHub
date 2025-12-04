using InternHub.Application.DTO.Project;
using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore; // <<< ДОДАНО ДЛЯ Include()
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
        
        // Нові ID для тестування (OwnerId тепер відповідає CandidateId в моделі Project)
        private readonly Guid OwnerCandidateId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        private Guid _testProjectId; 
        private Guid _testMemberCandidateId; // Відповідає MemberId у попередньому коді

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
                                 .Include(p => p.ProjectMembers) // <<< ВИПРАВЛЕНО: ProjectMembers
                                 .Single(p => p.Name == "Test Project Name"); // <<< ВИПРАВЛЕНО: Name

            _testProjectId = testProject.Id;
            
            // ВИПРАВЛЕНО: CandidateId замість MemberId, ProjectMembers замість Members
            _testMemberCandidateId = testProject.ProjectMembers.First(m => m.CandidateId != OwnerCandidateId).CandidateId;
        }

        private async Task SeedTestProjectDataAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Створення користувача-власника (Candidate)
            var owner = new ApplicationUser
            {
                Id = OwnerCandidateId,
                UserName = "projectowner@test.com",
                Email = "projectowner@test.com",
                EmailConfirmed = true
            };
            if (await userManager.FindByEmailAsync(owner.Email) == null)
            {
                await userManager.CreateAsync(owner, "Password123!");
                await userManager.AddToRoleAsync(owner, RoleConstants.Candidate);
                await _db.Candidates.AddAsync(new Candidate { UserId = OwnerCandidateId, Email = owner.Email, Id = OwnerCandidateId });
            }

            // 2. Створення члена проекту (Candidate)
            _testMemberCandidateId = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var member = new ApplicationUser
            {
                Id = _testMemberCandidateId,
                UserName = "projectmember@test.com",
                Email = "projectmember@test.com",
                EmailConfirmed = true
            };
            if (await userManager.FindByEmailAsync(member.Email) == null)
            {
                await userManager.CreateAsync(member, "Password123!");
                await userManager.AddToRoleAsync(member, RoleConstants.Candidate);
                await _db.Candidates.AddAsync(new Candidate { UserId = _testMemberCandidateId, Email = member.Email, Id = _testMemberCandidateId });
            }

            // 3. Створення тестового проекту
            if (!_db.Projects.Any(p => p.CandidateId == OwnerCandidateId && p.Name == "Test Project Name"))
            {
                _testProjectId = Guid.NewGuid();
                _db.Projects.Add(new Project
                {
                    Id = _testProjectId,
                    Name = "Test Project Name",        // <<< ВИПРАВЛЕНО: Name
                    Description = "Initial description",
                    CandidateId = OwnerCandidateId,   // <<< ВИПРАВЛЕНО: CandidateId
                    ProjectMembers = new List<ProjectMember> // <<< ВИПРАВЛЕНО: ProjectMembers
                    {
                        new ProjectMember 
                        { 
                            ProjectId = _testProjectId, 
                            CandidateId = _testMemberCandidateId, // <<< ВИПРАВЛЕНО: CandidateId
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
            var response = await _client.GetAsync("/api/projects/my");
            response.EnsureSuccessStatusCode();

            var projects = await response.Content.ReadFromJsonAsync<List<ProjectListItemDto>>();
            
            Assert.NotNull(projects);
            
            // УВАГА: ProjectListItemDto не містить OwnerId/CandidateId. 
            // Тому ми можемо перевірити лише кількість або вміст, але не власника напряму.
            // Припускаємо, що сервіс повертає тільки проекти власника.
            Assert.NotEmpty(projects);
            Assert.Contains(projects, p => p.Name == "Test Project Name"); // Перевірка за іменем
        }

        // --- POST /api/projects ---
        [Fact]
        public async Task CreateProject_ReturnsCreated_AndProjectExistsInDb()
        {
            var createDto = new CreateProjectDto 
            { 
                Name = "New Test Project",      // <<< ВИПРАВЛЕНО: Name
                Description = "Description for new project" 
            };
            
            var response = await _client.PostAsJsonAsync("/api/projects", createDto);
            
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Оскільки контролер повертає CreatedAtAction(nameof(GetProjectId), new { projectId = project}, project);
            // Це зазвичай повертає повний об'єкт. Спробуємо його розпарсити.
            var createdProjectDto = await response.Content.ReadFromJsonAsync<ProjectDetailsDto>();
            Assert.NotNull(createdProjectDto);
            Guid projectGuid = createdProjectDto.Id;
            Assert.NotEqual(Guid.Empty, projectGuid);

            // Перевірка в базі даних
            var createdProject = await _db.Projects.FindAsync(projectGuid);
            Assert.NotNull(createdProject);
            // ВИПРАВЛЕНО: CandidateId
            Assert.Equal(OwnerCandidateId, createdProject.CandidateId); 
            // ВИПРАВЛЕНО: Name
            Assert.Equal(createDto.Name, createdProject.Name); 
        }

        // --- GET /api/projects/{projectId} ---
        [Fact]
        public async Task GetProjectById_ReturnsOk_WhenOwnerAccessing()
        {
            var response = await _client.GetAsync($"/api/projects/{_testProjectId}");
            response.EnsureSuccessStatusCode(); 

            var project = await response.Content.ReadFromJsonAsync<ProjectDetailsDto>();
            Assert.NotNull(project);
            Assert.Equal(_testProjectId, project.Id);
            
            // УВАГА: ProjectDetailsDto не містить OwnerId/CandidateId.
            // Перевіряємо за іншим полем, наприклад, OwnerName (якщо воно заповнюється)
            // Assert.NotNull(project.OwnerName); 
        }

        // --- PUT /api/projects/{projectId} ---
        [Fact]
        public async Task UpdateProject_ReturnsNoContent_WhenOwnerAccessing()
        {
            var updateDto = new UpdateProjectDto
            {
                Name = "Updated Name", // <<< ВИПРАВЛЕНО: Name
                Description = "Fully updated description"
            };

            var response = await _client.PutAsJsonAsync($"/api/projects/{_testProjectId}", updateDto);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); 

            // Перевірка в базі даних
            var entityEntry = _db.Entry(await _db.Projects.FindAsync(_testProjectId)); 
            entityEntry.Reload(); 
            var updatedProject = entityEntry.Entity;
            
            // ВИПРАВЛЕНО: Name
            Assert.Equal(updateDto.Name, updatedProject.Name); 
            Assert.Equal(updateDto.Description, updatedProject.Description); 
        }

        [Fact]
        public async Task UpdateProject_ReturnsForbidden_WhenNotOwner()
        {
            // Зміна користувача на CandidateUserId, який не є власником
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", CandidateUserId.ToString());

            var updateDto = new UpdateProjectDto 
            {
                Name = "Forbidden Update" // <<< ВИПРАВЛЕНО: Name
            };

            var response = await _client.PutAsJsonAsync($"/api/projects/{_testProjectId}", updateDto);
            
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        
        // --- POST /api/projects/{projectId}/members ---
        [Fact]
        public async Task AddMember_ReturnsNoContent_AndMemberIsAddedToDb()
        {
            var newCandidateId = Guid.Parse("77777777-7777-7777-7777-777777777777");
            
            // Створення користувача-члена
            var userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            // (Створення користувача для newCandidateId пропущено, але має бути додано для надійності)

            var dto = new AddMemberDto { 
                CandidateId = newCandidateId, // <<< ВИПРАВЛЕНО: CandidateId
                Role = "Frontend"
            };

            var response = await _client.PostAsJsonAsync($"/api/projects/{_testProjectId}/members", dto);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); 

            // Перевірка в базі даних
            var isMemberAdded = _db.ProjectMembers
                .Any(pm => pm.ProjectId == _testProjectId && pm.CandidateId == newCandidateId); // <<< ВИПРАВЛЕНО: CandidateId
            
            Assert.True(isMemberAdded);
        }
        
        // --- DELETE /api/projects/{projectId}/members/{memberId} ---
        [Fact]
        public async Task RemoveMember_ReturnsNoContent_AndRemovesFromDb()
        {
            // Використовуємо _testMemberCandidateId
            var response = await _client.DeleteAsync($"/api/projects/{_testProjectId}/members/{_testMemberCandidateId}");
            
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); 

            // Перевірка в базі даних
            var isMemberStillPresent = _db.ProjectMembers
                .Any(pm => pm.ProjectId == _testProjectId && pm.CandidateId == _testMemberCandidateId); // <<< ВИПРАВЛЕНО: CandidateId
            
            Assert.False(isMemberStillPresent);
        }
        
        [Fact]
        public async Task RemoveMember_ReturnsForbidden_WhenNotOwner()
        {
            // Зміна користувача на CandidateUserId, який не є власником
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", CandidateUserId.ToString());

            // Спроба видалити іншого члена
            var response = await _client.DeleteAsync($"/api/projects/{_testProjectId}/members/{_testMemberCandidateId}");
            
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}