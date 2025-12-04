using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using InternHub.Application.DTO.Project;
using InternHub.Infrastructure;
using InternHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InternHub.IntegrationTests.Controllers
{
    public class ProjectControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        
        // Тестові дані, які будуть використовуватися в тестах
        private readonly Guid _testOwnerCandidateId = new Guid("4b123456-7890-abcd-ef01-234567890123");
        private readonly Guid _otherCandidateId = new Guid("5c234567-8901-bcde-fg12-345678901234");
        private const string TestEmail = "test.owner@example.com";
        private const string TestPassword = "Password123!";
        private const string TestRole = "Candidate";

        public ProjectControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            
            // Створення клієнта з автентифікацією
            _client = _factory.CreateClient();
            // Ми використовуємо Candidate Id як ClaimTypes.NameIdentifier (UserId)
            _client.DefaultRequestHeaders.Add("X-Test-UserId", _testOwnerCandidateId.ToString()); 
            _client.DefaultRequestHeaders.Add("X-Test-Role", TestRole);
        }

        // --- Методи підготовки та очищення бази даних ---

        // Використовуємо Scoped ServiceProvider для доступу до DbContext/UserManager
        private async Task SeedDataAsync(InternHubDbContext context, UserManager<ApplicationUser> userManager)
        {
            // 1. Створення ApplicationUser та Candidate для власника
            var ownerUser = new ApplicationUser
            {
                Id = _testOwnerCandidateId, // Id Candidate та User збігаються для зручності
                Email = TestEmail,
                UserName = TestEmail, // 🚨 ВИПРАВЛЕНО: UserName належить ApplicationUser
                Name = "Test",         // 🚨 ВИПРАВЛЕНО: FirstName => Name
                Surname = "Owner",       // 🚨 ВИПРАВЛЕНО: LastName => Surname
                EmailConfirmed = true
            };
            
            var ownerCandidate = new Candidate
            {
                Id = _testOwnerCandidateId,
                UserId = _testOwnerCandidateId,
                Email = TestEmail
                // Властивості FirstName/LastName/UserName відсутні у моделі Candidate
            };
            
            // Перевірка існування користувача та його створення
            if (await userManager.FindByIdAsync(ownerUser.Id.ToString()) == null)
            {
                var createResult = await userManager.CreateAsync(ownerUser, TestPassword); // 🚨 ВИПРАВЛЕНО: Передаємо ApplicationUser
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(ownerUser, TestRole); // 🚨 ВИПРАВЛЕНО: Передаємо ApplicationUser
                    // Зв'язуємо Candidate з User та зберігаємо Candidate
                    context.Candidates.Add(ownerCandidate);
                }
            }

            // 2. Створення ApplicationUser та Candidate для іншого кандидата
            var otherUser = new ApplicationUser
            {
                Id = _otherCandidateId,
                Email = "other@example.com",
                UserName = "other@example.com", // 🚨 ВИПРАВЛЕНО: UserName належить ApplicationUser
                Name = "Other",
                Surname = "User",
                EmailConfirmed = true
            };
            
            var otherCandidate = new Candidate
            {
                Id = _otherCandidateId,
                UserId = _otherCandidateId,
                Email = "other@example.com"
            };
            
            if (await userManager.FindByIdAsync(otherUser.Id.ToString()) == null)
            {
                var createResult = await userManager.CreateAsync(otherUser, TestPassword); // 🚨 ВИПРАВЛЕНО: Передаємо ApplicationUser
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(otherUser, TestRole); // 🚨 ВИПРАВЛЕНО: Передаємо ApplicationUser
                    context.Candidates.Add(otherCandidate);
                }
            }
            
            await context.SaveChangesAsync();
        }

        private async Task CleanDataAsync(InternHubDbContext context)
        {
            // Очищення даних, пов'язаних з тестами
            context.Projects.RemoveRange(context.Projects.Where(p => p.CandidateId == _testOwnerCandidateId || p.CandidateId == _otherCandidateId));
            
            var projectIds = context.Projects.Select(p => p.Id);
            var membersToRemove = context.ProjectMembers.Where(pm => projectIds.Contains(pm.ProjectId) || pm.CandidateId == _testOwnerCandidateId || pm.CandidateId == _otherCandidateId);
            context.ProjectMembers.RemoveRange(membersToRemove);

            // Очищення TeamRequests (якщо потрібно, уникнення помилок ForeignKey)
            var requestsToRemove = context.TeamRequests.Where(tr => tr.CandidateId == _testOwnerCandidateId || tr.CandidateId == _otherCandidateId);
            context.TeamRequests.RemoveRange(requestsToRemove);
            
            await context.SaveChangesAsync();
        }

        // ------------------------------------------------------------------
        // POST /api/projects
        // ------------------------------------------------------------------

        [Fact]
        public async Task CreateProject_ReturnsCreated_AndProjectExistsInDb()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InternHubDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await SeedDataAsync(context, userManager);
            
            var createDto = new CreateProjectDto 
            { 
                Name = "Test Project for DB", 
                Description = "A description",
                // 🚨 ВИПРАВЛЕНО: Видалено 'Technologies = new List<Guid>()' оскільки його немає у моделі DTO
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(createDto),
                System.Text.Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/projects", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            
            var newProjectId = JsonConvert.DeserializeObject<Guid>(responseString); 
            
            // Перевірка, що об'єкт справді створено у базі даних
            var projectInDb = await context.Projects
                                           .FirstOrDefaultAsync(p => p.Id == newProjectId && p.CandidateId == _testOwnerCandidateId);
            
            Assert.NotNull(projectInDb);
            Assert.Equal(createDto.Name, projectInDb.Name);
            
            await CleanDataAsync(context);
        }

        // ------------------------------------------------------------------
        // GET /api/projects/my
        // ------------------------------------------------------------------

        [Fact]
        public async Task GetProjects_ReturnsOnlyOwnersProjects()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InternHubDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await SeedDataAsync(context, userManager);
            
            // Створення проекту власника
            context.Projects.Add(new Project { Id = Guid.NewGuid(), Name = "My Project 1", CandidateId = _testOwnerCandidateId });
            // Створення чужого проекту
            context.Projects.Add(new Project { Id = Guid.NewGuid(), Name = "Other Project 2", CandidateId = _otherCandidateId });
            await context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/projects/my");

            // Assert
            response.EnsureSuccessStatusCode(); 
            var responseString = await response.Content.ReadAsStringAsync();
            var projects = JsonConvert.DeserializeObject<List<ProjectListItemDto>>(responseString);

            Assert.NotNull(projects);
            Assert.Single(projects); 
            Assert.Equal("My Project 1", projects[0].Name);
            
            await CleanDataAsync(context);
        }
        
        // ------------------------------------------------------------------
        // DELETE /api/projects/{projectId}/members/{memberId}
        // ------------------------------------------------------------------

        [Fact]
        public async Task RemoveMember_ReturnsNoContent_AndRemovesFromDb()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InternHubDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await SeedDataAsync(context, userManager);

            var projectId = Guid.NewGuid();
            var memberId = _otherCandidateId;
            
            // 1. Створення проекту та члена команди
            context.Projects.Add(new Project { Id = projectId, Name = "Team Project", CandidateId = _testOwnerCandidateId });
            context.ProjectMembers.Add(new ProjectMember { ProjectId = projectId, CandidateId = memberId });
            await context.SaveChangesAsync();
            
            var memberBefore = await context.ProjectMembers.SingleOrDefaultAsync(pm => pm.ProjectId == projectId && pm.CandidateId == memberId);
            Assert.NotNull(memberBefore);

            // Act
            var response = await _client.DeleteAsync($"/api/projects/{projectId}/members/{memberId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var memberAfter = await context.ProjectMembers.SingleOrDefaultAsync(pm => pm.ProjectId == projectId && pm.CandidateId == memberId);
            Assert.Null(memberAfter);
            
            await CleanDataAsync(context);
        }
        
        // ------------------------------------------------------------------
        // PUT /api/projects/{projectId} - Тест на авторизацію (відмова)
        // ------------------------------------------------------------------

        [Fact]
        public async Task UpdateProject_ReturnsForbidden_WhenNotOwner()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InternHubDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await SeedDataAsync(context, userManager);
            
            var projectId = Guid.NewGuid();
            
            // Створення проекту, де власником є _otherCandidateId
            context.Projects.Add(new Project { Id = projectId, Name = "Not Mine", CandidateId = _otherCandidateId });
            await context.SaveChangesAsync();
            
            var updateDto = new UpdateProjectDto { Name = "Try to update" };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(updateDto),
                System.Text.Encoding.UTF8,
                "application/json");

            // Act: _client автентифікований як _testOwnerCandidateId, але проект належить _otherCandidateId
            var response = await _client.PutAsync($"/api/projects/{projectId}", jsonContent);

            // Assert
            // Очікується 403 Forbidden, оскільки логіка UpdateProjectAsync має забороняти доступ
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            
            await CleanDataAsync(context);
        }
    }
}