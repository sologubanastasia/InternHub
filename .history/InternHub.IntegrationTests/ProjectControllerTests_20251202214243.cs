using InternHub.Application.DTO.Project;
using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
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
    // тестів, які змінюють одну й ту саму базу даних (якщо ви не ізолюєте їх)
    [Collection("Sequential")] 
    public class ProjectControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly InternHubDbContext _db;
        
        // Фіксовані ID, що використовуються в SeedTestDataAsync
        private readonly Guid CompanyUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private readonly Guid CandidateUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private readonly Guid TestJobId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        
        // Нові ID для тестів
        private readonly Guid OwnerId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        private Guid _testProjectId; // Буде ініціалізовано в SeedTestProjectDataAsync
        private Guid _testMemberId;

        public ProjectControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            var scope = factory.Services.CreateScope();
            _db = scope.ServiceProvider.GetRequiredService<InternHubDbContext>();

            // Цей код (імітація аутентифікації) повинен був бути на рядку 25 у вашому попередньому виводі помилок.
            _client.DefaultRequestHeaders.Add("X-Test-UserId", OwnerId.ToString()); 
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Candidate");

            // Переконайтеся, що тестові дані для цього контролера існують
            SeedTestProjectDataAsync(scope.ServiceProvider).GetAwaiter().GetResult();
            
            // Ініціалізація змінних, якщо вони були створені
            _testProjectId = _db.Projects.Single(p => p.Title == "Test Project Title").Id;
            _testMemberId = _db.Projects.Single(p => p.Title == "Test Project Title")
                                       .Members.First(m => m.MemberId != OwnerId).MemberId;
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
                await _db.Candidates.AddAsync(new Candidate { UserId = _testMemberId, Email = member.Email, Id = Guid.NewGuid() });
            }

            // 3. Створення тестового проекту
            if (!_db.Projects.Any(p => p.OwnerId == OwnerId))
            {
                _testProjectId = Guid.NewGuid();
                _db.Projects.Add(new Project
                {
                    Id = _testProjectId,
                    Title = "Test Project Title",
                    Description = "Initial description",
                    OwnerId = OwnerId,
                    Members = new List<ProjectMember>
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
            
            // Перевірка, що всі проекти належать OwnerId
            Assert.True(projects.All(p => p.OwnerId == OwnerId));
        }

        // --- POST /api/projects ---
        [Fact]
        public async Task CreateProject_ReturnsCreated_AndProjectExistsInDb()
        {
            var createDto = new CreateProjectDto 
            { 
                Title = "New Test Project", 
                Description = "Description for new project" 
            };
            
            var response = await _client.PostAsJsonAsync("/api/projects", createDto);
            
            // Очікуємо 201 Created
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var projectGuid = await response.Content.ReadFromJsonAsync<Guid>();
            Assert.NotEqual(Guid.Empty, projectGuid);

            // Перевірка в базі даних
            var createdProject = await _db.Projects.FindAsync(projectGuid);
            Assert.NotNull(createdProject);
            Assert.Equal(OwnerId, createdProject.OwnerId);
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
            Assert.Equal(OwnerId, project.OwnerId);
        }

        // --- PUT /api/projects/{projectId} ---
        [Fact]
        public async Task UpdateProject_ReturnsNoContent_WhenOwnerAccessing()
        {
            var updateDto = new UpdateProjectDto
            {
                Title = "Updated Title",
                Description = "Fully updated description"
            };

            var response = await _client.PutAsJsonAsync($"/api/projects/{_testProjectId}", updateDto);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); // Очікуємо 204 No Content

            // Перевірка в базі даних
            _db.Entry(_db.Projects.Find(_testProjectId)).Reload(); // Оновлення даних з БД
            var updatedProject = await _db.Projects.FindAsync(_testProjectId);
            
            Assert.Equal(updateDto.Title, updatedProject.Title);
            Assert.Equal(updateDto.Description, updatedProject.Description);
        }

        [Fact]
        public async Task UpdateProject_ReturnsForbidden_WhenNotOwner()
        {
            // Зміна користувача на CandidateUserId, який не є власником _testProjectId
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", CandidateUserId.ToString());

            var updateDto = new UpdateProjectDto { Title = "Forbidden Update" };

            var response = await _client.PutAsJsonAsync($"/api/projects/{_testProjectId}", updateDto);
            
            // Очікуємо 403 Forbidden
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        
        // --- POST /api/projects/{projectId}/members ---
        [Fact]
        public async Task AddMember_ReturnsNoContent_AndMemberIsAddedToDb()
        {
            var newMemberId = Guid.Parse("77777777-7777-7777-7777-777777777777");
            
            // Створення користувача-члена
            var member = new ApplicationUser
            {
                Id = newMemberId,
                UserName = "newmember@test.com",
                Email = "newmember@test.com",
                EmailConfirmed = true
            };
            var userManager = _db.GetService<UserManager<ApplicationUser>>();
            if (await userManager.FindByEmailAsync(member.Email) == null)
            {
                await userManager.CreateAsync(member, "Password123!");
                await userManager.AddToRoleAsync(member, RoleConstants.Candidate);
                await _db.Candidates.AddAsync(new Candidate { UserId = newMemberId, Email = member.Email, Id = Guid.NewGuid() });
                await _db.SaveChangesAsync();
            }

            var dto = new AddMemberDto { MemberId = newMemberId };

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
            
            // Очікуємо 403 Forbidden
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}