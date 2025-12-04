using System;
using System.Collections.Generic;
using System.Linq; // Додано для використання SingleOrDefaultAsync та FirstOrDefaultAsync
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
using Microsoft.EntityFrameworkCore; // 🚨 ДОДАНО: Необхідно для методів EF Core Async

namespace InternHub.IntegrationTests.Controllers
{
    // IClassFixture використовує CustomWebApplicationFactory, який забезпечує
    // налаштування тестового середовища та DbContext.
    public class ProjectControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        
        // Тестові дані, які будуть використовуватися в тестах
        private readonly Guid _testOwnerId = new Guid("4b123456-7890-abcd-ef01-234567890123");
        private readonly Guid _otherCandidateId = new Guid("5c234567-8901-bcde-fg12-345678901234");
        private const string TestEmail = "test.candidate@example.com";
        private const string TestPassword = "Password123!";
        private const string TestRole = "Candidate";

        // 🚨 ВИПРАВЛЕНО: Ім'я конструктора має відповідати імені класу (ProjectControllerTests)
        public ProjectControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            
            // Створення клієнта з автентифікацією
            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", _testOwnerId.ToString());
            _client.DefaultRequestHeaders.Add("X-Test-Role", TestRole);
        }

        // --- Методи підготовки та очищення бази даних ---

        // Використовуємо Scoped ServiceProvider для доступу до DbContext/UserManager
        private async Task SeedDataAsync(InternHubDbContext context, UserManager<ApplicationUser> userManager)
        {
            // 1. Створення тестового користувача (власника)
            var owner = new Candidate
            {
                Id = _testOwnerId,
                Email = TestEmail,
                UserName = TestEmail,
                FirstName = "Test",
                LastName = "Owner"
            };
            
            // Використовуємо FindByIdAsync для перевірки існування
            if (await userManager.FindByIdAsync(owner.Id.ToString()) == null)
            {
                await userManager.CreateAsync(owner, TestPassword);
                await userManager.AddToRoleAsync(owner, TestRole);
            }

            // 2. Створення іншого кандидата
            var otherCandidate = new Candidate
            {
                Id = _otherCandidateId,
                Email = "other@example.com",
                UserName = "other@example.com",
                FirstName = "Other",
                LastName = "User"
            };
            
            // Використовуємо FindByIdAsync для перевірки існування
            if (await userManager.FindByIdAsync(otherCandidate.Id.ToString()) == null)
            {
                await userManager.CreateAsync(otherCandidate, TestPassword);
                await userManager.AddToRoleAsync(otherCandidate, TestRole);
            }
            
            // Зберігати зміни не потрібно, якщо користувачі створені через UserManager
            // await context.SaveChangesAsync(); 
        }

        // Очищення даних, специфічних для тесту (зазвичай це робиться у CustomWebApplicationFactory)
        private async Task CleanDataAsync(InternHubDbContext context)
        {
            // Очищення даних Project, ProjectMember, TeamRequest і т.д.
            
            // 🚨 ВАЖЛИВО: Очищаємо лише ті дані, які стосуються логіки тесту
            // Використання EF Core для пакетного видалення (якщо підтримується провайдером)
            // або видалення через DbSet:
            context.Projects.RemoveRange(context.Projects.Where(p => p.CandidateId == _testOwnerId || p.CandidateId == _otherCandidateId));
            
            // Видаляємо всі ProjectMembers, пов'язані з тестовими проектами/користувачами
            var projectIds = context.Projects.Select(p => p.Id);
            var membersToRemove = context.ProjectMembers.Where(pm => projectIds.Contains(pm.ProjectId) || pm.CandidateId == _testOwnerId || pm.CandidateId == _otherCandidateId);
            context.ProjectMembers.RemoveRange(membersToRemove);
            
            // Якщо використовується EnsureDeleted/EnsureCreated у фабриці, цей крок може бути менш детальним,
            // але ручне очищення забезпечує кращу ізоляцію тесту.
            
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
                Technologies = new List<Guid>() 
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
            
            // Контролер повертає 'Guid' як результат 'CreatedAtAction',
            // тому десеріалізуємо його як Guid.
            var newProjectId = JsonConvert.DeserializeObject<Guid>(responseString); 
            
            // Перевірка, що об'єкт справді створено у базі даних
            var projectInDb = await context.Projects
                                           .FirstOrDefaultAsync(p => p.Id == newProjectId && p.CandidateId == _testOwnerId);
            
            Assert.NotNull(projectInDb);
            Assert.Equal(createDto.Name, projectInDb.Name);
            
            await CleanDataAsync(context); // Очищення після тесту
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
            context.Projects.Add(new Project { Id = Guid.NewGuid(), Name = "My Project 1", CandidateId = _testOwnerId });
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
            Assert.Single(projects); // Тільки один проект належить власнику
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
            context.Projects.Add(new Project { Id = projectId, Name = "Team Project", CandidateId = _testOwnerId });
            context.ProjectMembers.Add(new ProjectMember { ProjectId = projectId, CandidateId = memberId });
            await context.SaveChangesAsync();
            
            // Перевірка, що член команди існує до видалення
            var memberBefore = await context.ProjectMembers.SingleOrDefaultAsync(pm => pm.ProjectId == projectId && pm.CandidateId == memberId);
            Assert.NotNull(memberBefore);

            // Act
            var response = await _client.DeleteAsync($"/api/projects/{projectId}/members/{memberId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Перевірка, що член команди був видалений з бази даних
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

            // Act: _client автентифікований як _testOwnerId, але проект належить _otherCandidateId
            var response = await _client.PutAsync($"/api/projects/{projectId}", jsonContent);

            // Assert
            // Очікується 403 Forbidden, оскільки логіка UpdateProjectAsync має забороняти доступ
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            
            await CleanDataAsync(context);
        }
    }
}