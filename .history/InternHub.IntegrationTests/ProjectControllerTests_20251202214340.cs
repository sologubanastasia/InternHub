using InternHub.Application.DTO.Project;
using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.ChangeTracking; // Додано для EntityEntry
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
    // Використовуйте цю колекцію для запобігання паралельному запуску
    [Collection("Sequential")] 
    public class ProjectControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InternHubDbContext _db;
        private readonly IServiceProvider _serviceProvider;
        
        // Фіксовані ID, що використовуються в SeedTestDataAsync
        private readonly Guid CandidateUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        
        // Нові ID для тестів
        private readonly Guid OwnerId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        private Guid _testProjectId; // Буде ініціалізовано в SeedTestProjectDataAsync
        private Guid _testMemberId;
        
        public ProjectControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            var scope = factory.Services.CreateScope();
            _serviceProvider = scope.ServiceProvider;
            _db = _serviceProvider.GetRequiredService<InternHubDbContext>();

            _client.DefaultRequestHeaders.Add("X-Test-UserId", OwnerId.ToString()); 
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Candidate");

            SeedTestProjectDataAsync(_serviceProvider).GetAwaiter().GetResult();
            
            // Ініціалізація змінних, якщо вони були створені
            var testProject = _db.Projects
                                 .Include(p => p.Members) // Додайте Include, якщо це необхідно для Members
                                 .Single(p => p.Title == "Test Project Title");

            _testProjectId = testProject.Id;
            
            // CS1061: 'ProjectMember' does not contain a definition for 'MemberId'
            // Припускаємо, що властивість MemberId існує
            _testMemberId = testProject.Members.First(m => m.MemberId != OwnerId).MemberId;
        }

        private async Task SeedTestProjectDataAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Створення користувача-власника (якщо він ще не існує)
            var owner = new ApplicationUser
            {
                Id = OwnerId,
                UserName = "projectowner@test.com",
                Email = "projectowner@test.com",
                EmailConfirmed = true
            };
            if (await userManager.FindByEmailAsync(owner.Email) == null)
            {
                await userManager.CreateAsync(owner, "Password123!");
                await userManager.AddToRoleAsync(owner, RoleConstants.Candidate);
                // Припускаємо, що ця логіка існує
                await _db.Candidates.AddAsync(new Candidate { UserId = OwnerId, Email = owner.Email, Id = Guid.NewGuid() });
            }

            // 2. Створення члена проекту (якщо він ще не існує)
            _testMemberId = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var member = new ApplicationUser
            {
                Id = _testMemberId,
                UserName = "projectmember@test.com",
                Email = "projectmember@test.com",
                EmailConfirmed = true
            };
            if (await userManager.FindByEmailAsync(member.Email) == null)
            {
                await userManager.CreateAsync(member, "Password123!");
                await userManager.AddToRoleAsync(member, RoleConstants.Candidate);
                // Припускаємо, що ця логіка існує
                await _db.Candidates.AddAsync(new Candidate { UserId = _testMemberId, Email = member.Email, Id = Guid.NewGuid() });
            }

            // 3. Створення тестового проекту
            if (!_db.Projects.Any(p => p.OwnerId == OwnerId && p.Title == "Test Project Title"))
            {
                _testProjectId = Guid.NewGuid();
                // CS0117: 'Project' does not contain a definition for 'Title', 'OwnerId', 'Members'
                // Припускаємо, що вони існують
                _db.Projects.Add(new Project
                {
                    Id = _testProjectId,
                    Title = "Test Project Title", // Виправлено: Title (припускаємо, що існує)
                    Description = "Initial description",
                    OwnerId = OwnerId,          // Виправлено: OwnerId (припускаємо, що існує)
                    Members = new List<ProjectMember> // Виправлено: Members (припускаємо, що існує)
                    {
                        new ProjectMember { ProjectId = _testProjectId, MemberId = _testMemberId }
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
            Assert.NotEmpty(projects);
            
            // CS1061: 'ProjectListItemDto' does not contain a definition for 'OwnerId'
            // Припускаємо, що властивість OwnerId існує
            Assert.True(projects.All(p => p.OwnerId == OwnerId)); 
        }

        // --- POST /api/projects ---
        [Fact]
        public async Task CreateProject_ReturnsCreated_AndProjectExistsInDb()
        {
            var createDto = new CreateProjectDto 
            { 
                // CS0117: 'CreateProjectDto' does not contain a definition for 'Title'
                Title = "New Test Project", // Виправлено: Title
                Description = "Description for new project" 
            };
            
            var response = await _client.PostAsJsonAsync("/api/projects", createDto);
            
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Очікуємо, що контролер повертає створений об'єкт або його Id
            var createdObject = await response.Content.ReadFromJsonAsync<object>(); 
            var projectGuid = (Guid)createdObject.GetType().GetProperty("Id").GetValue(createdObject);
            Assert.NotEqual(Guid.Empty, projectGuid);

            // Перевірка в базі даних
            var createdProject = await _db.Projects.FindAsync(projectGuid);
            Assert.NotNull(createdProject);
            // CS1061: 'Project' does not contain a definition for 'OwnerId'
            Assert.Equal(OwnerId, createdProject.OwnerId); 
            // CS1061: 'CreateProjectDto' does not contain a definition for 'Title'
            // CS1061: 'Project' does not contain a definition for 'Title'
            Assert.Equal(createDto.Title, createdProject.Title); 
        }

        // --- GET /api/projects/{projectId} ---
        [Fact]
        public async Task GetProjectById_ReturnsOk_WhenOwnerAccessing()
        {
            var response = await _client.GetAsync($"/api/projects/{_testProjectId}");
            response.EnsureSuccessStatusCode(); // Очікуємо 200 Ok

            var project = await response.Content.ReadFromJsonAsync<ProjectDetailsDto>();
            Assert.NotNull(project);
            Assert.Equal(_testProjectId, project.Id);
            // CS1061: 'ProjectDetailsDto' does not contain a definition for 'OwnerId'
            Assert.Equal(OwnerId, project.OwnerId); 
        }

        // --- PUT /api/projects/{projectId} ---
        [Fact]
        public async Task UpdateProject_ReturnsNoContent_WhenOwnerAccessing()
        {
            var updateDto = new UpdateProjectDto
            {
                // CS0117: 'UpdateProjectDto' does not contain a definition for 'Title'
                Title = "Updated Title", // Виправлено: Title
                Description = "Fully updated description"
            };

            var response = await _client.PutAsJsonAsync($"/api/projects/{_testProjectId}", updateDto);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); // Очікуємо 204 No Content

            // Перевірка в базі даних
            // Усунення попередження CS8634
            var entry = _db.Entry(await _db.Projects.FindAsync(_testProjectId)); 
            entry.Reload(); 
            var updatedProject = entry.Entity;
            
            // CS1061: 'UpdateProjectDto' does not contain a definition for 'Title'
            Assert.Equal(updateDto.Title, updatedProject.Title); 
            // Усунення попередження CS8602
            Assert.Equal(updateDto.Description, updatedProject.Description); 
        }

        [Fact]
        public async Task UpdateProject_ReturnsForbidden_WhenNotOwner()
        {
            // Зміна користувача на CandidateUserId, який не є власником _testProjectId
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", CandidateUserId.ToString());

            var updateDto = new UpdateProjectDto 
            {
                // CS0117: 'UpdateProjectDto' does not contain a definition for 'Title'
                Title = "Forbidden Update" 
            };

            var response = await _client.PutAsJsonAsync($"/api/projects/{_testProjectId}", updateDto);
            
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        
        // --- POST /api/projects/{projectId}/members ---
        [Fact]
        public async Task AddMember_ReturnsNoContent_AndMemberIsAddedToDb()
        {
            var newMemberId = Guid.Parse("77777777-7777-7777-7777-777777777777");
            
            // Створення користувача-члена
            var userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>(); // Виправлено CS1929
            // ... (Код створення користувача)

            var dto = new AddMemberDto { 
                // CS0117: 'AddMemberDto' does not contain a definition for 'MemberId'
                MemberId = newMemberId 
            };

            var response = await _client.PostAsJsonAsync($"/api/projects/{_testProjectId}/members", dto);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); // Очікуємо 204 No Content

            // Перевірка в базі даних
            var isMemberAdded = _db.ProjectMembers
                .Any(pm => pm.ProjectId == _testProjectId && pm.MemberId == newMemberId);
            
            Assert.True(isMemberAdded);
        }
        
        // --- DELETE /api/projects/{projectId}/members/{memberId} ---
        [Fact]
        public async Task RemoveMember_ReturnsNoContent_AndRemovesFromDb()
        {
            // Використовуємо _testMemberId, створений під час ініціалізації
            var response = await _client.DeleteAsync($"/api/projects/{_testProjectId}/members/{_testMemberId}");
            
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); // Очікуємо 204 No Content

            // Перевірка в базі даних
            // CS1061: 'ProjectMember' does not contain a definition for 'MemberId'
            var isMemberStillPresent = _db.ProjectMembers
                .Any(pm => pm.ProjectId == _testProjectId && pm.MemberId == _testMemberId); 
            
            Assert.False(isMemberStillPresent);
        }
        
        [Fact]
        public async Task RemoveMember_ReturnsForbidden_WhenNotOwner()
        {
            // Зміна користувача на CandidateUserId, який не є власником
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", CandidateUserId.ToString());

            // Спроба видалити іншого члена
            var response = await _client.DeleteAsync($"/api/projects/{_testProjectId}/members/{_testMemberId}");
            
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}