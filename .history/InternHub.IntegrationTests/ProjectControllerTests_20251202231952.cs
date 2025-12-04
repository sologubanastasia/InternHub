using InternHub.Application.DTO.Project;
using InternHub.Application.DTO.Technology;
using Microsoft.Extensions.DependencyInjection;
using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore; 

namespace InternHub.IntegrationTests
{
    // Забезпечує послідовне виконання всіх тестів у цій колекції.
    [Collection("Sequential")] 
    public class ProjectControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        // Визначені тестові ID з CustomWebApplicationFactory
        private readonly Guid _projectOwnerId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        private readonly Guid _projectMemberId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        private readonly Guid _candidateUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        
        // Змінні для збереження стану між тестами
        private static Guid _testProjectId = Guid.Empty; 
        private static Guid _techId1 = Guid.Empty; 
        private static Guid _techId2 = Guid.Empty; 

        public ProjectControllerTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _output = output;

            // Ініціалізація допоміжних даних (технологій)
            if (_techId1 == Guid.Empty)
            {
                SeedTechnologies();
            }
        }
        
        // 💡 Допоміжний метод для діагностики HTTP помилок
        private async Task EnsureSuccessOrOutputError(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }
            
            var content = await response.Content.ReadAsStringAsync();
            
            // Якщо це 500, виводимо вміст для діагностики
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                _output.WriteLine($"--- 🚨 INTERNAL SERVER ERROR DIAGNOSTIC (500) ---");
                _output.WriteLine($"Request: {response.RequestMessage.Method} {response.RequestMessage.RequestUri}");
                _output.WriteLine($"Content: {content}");
                _output.WriteLine($"---------------------------------------------------");
            }
            // Кидаємо виняток, як і response.EnsureSuccessStatusCode(), але з додатковим контекстом
            throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}). Diagnostic content: {content}", null, response.StatusCode);
        }

        // 💡 Виправлений допоміжний метод: якщо ID не знайдено, намагаємося створити проект.
        private void EnsureTestProjectId()
        {
            if (_testProjectId == Guid.Empty)
            {
                using var scope = _factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<InternHubDbContext>();
                
                // 1. Спроба знайти проект у базі (якщо Test01 пройшов, але ID не було збережено)
                var project = context.Projects
                    .FirstOrDefault(p => p.CandidateId == _projectOwnerId);

                if (project != null)
                {
                    _testProjectId = project.Id;
                    return; // Успішно знайдено
                }
                
                // 2. Якщо проект не знайдено, примусово створюємо його, викликаючи Test01 синхронно.
                try
                {
                    _output.WriteLine("Project ID missing. Attempting to create project via Test01 logic.");
                    // Test01 виконується синхронно
                    Test01_CreateProject_ShouldReturnCreatedAndSetOwner().GetAwaiter().GetResult(); 
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to set up Test Project ID. Test01 failed: {ex.Message}");
                }
                
                // Після успішного виклику Test01, _testProjectId має бути встановлено.
                if (_testProjectId == Guid.Empty)
                {
                    throw new InvalidOperationException("Test Project ID is still missing after execution of Test01 logic.");
                }
            }
        }

        private void SeedTechnologies()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InternHubDbContext>();
            
            var tech1 = context.Technologies.FirstOrDefault(t => t.Name == "C#");
            var tech2 = context.Technologies.FirstOrDefault(t => t.Name == "Vue.js");

            if (tech1 == null)
            {
                tech1 = new Technology { Id = Guid.NewGuid(), Name = "C#" };
                context.Technologies.Add(tech1);
            }
            if (tech2 == null)
            {
                tech2 = new Technology { Id = Guid.NewGuid(), Name = "Vue.js" };
                context.Technologies.Add(tech2);
            }
            context.SaveChanges();

            _techId1 = tech1.Id;
            _techId2 = tech2.Id;
        }

        // --- Сценарії створення та отримання ---

        [Fact]
        public async Task Test01_CreateProject_ShouldReturnCreatedAndSetOwner()
        {
            // Якщо ID вже встановлено, виходимо, щоб уникнути дублювання запису.
            if (_testProjectId != Guid.Empty) return; 

            // Arrange
            var dto = new CreateProjectDto
            {
                Name = "Test Project Alpha",
                Description = "A description for testing.",
                IsTeamSearchActive = true,
                ProjectTechnologies = new List<Guid> { _techId1, _techId2 }
            };
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

            // Act: Встановлюємо заголовок X-Test-UserId
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", _projectOwnerId.ToString());
            var response = await _client.PostAsync("/api/projects", content);

            // Assert
            await EnsureSuccessOrOutputError(response); // 💡 Діагностика 500
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var projectId = JsonConvert.DeserializeObject<Guid>(responseContent);
            
            Assert.NotEqual(Guid.Empty, projectId);
            _testProjectId = projectId; // Зберігаємо ID для подальших тестів
        }
        
        [Fact]
        public async Task Test02_GetMyProjects_ShouldReturnOnlyOwnersProjects()
        {
            // Arrange
            EnsureTestProjectId(); 
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", _projectOwnerId.ToString());

            // Act
            var response = await _client.GetAsync("/api/projects/my");
            
            // Assert
            await EnsureSuccessOrOutputError(response); // 💡 Діагностика 500
            var content = await response.Content.ReadAsStringAsync();
            var projects = JsonConvert.DeserializeObject<List<ProjectListItemDto>>(content);

            Assert.Contains(projects, p => p.Id == _testProjectId);
        }

        [Fact]
        public async Task Test03_GetProjectDetails_AsOwner_ShouldReturnOk()
        {
            // Arrange
            EnsureTestProjectId(); 
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", _projectOwnerId.ToString());

            // Act
            var response = await _client.GetAsync($"/api/projects/{_testProjectId}");

            // Assert
            await EnsureSuccessOrOutputError(response); // 💡 Діагностика 500
            var content = await response.Content.ReadAsStringAsync();
            var details = JsonConvert.DeserializeObject<ProjectDetailsDto>(content);

            Assert.Equal("Test Project Alpha", details.Name);
            Assert.Contains(details.ProjectMembers, m => m.CandidateId == _projectOwnerId && m.Role == "Owner");
        }

        [Fact]
        public async Task Test04_GetProjectDetails_AsUnauthorizedCandidate_ShouldReturnForbidden()
        {
            // Arrange 
            EnsureTestProjectId(); 
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", _candidateUserId.ToString());

            // Act
            var response = await _client.GetAsync($"/api/projects/{_testProjectId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        
        // --- Сценарії оновлення ---

        [Fact]
        public async Task Test05_UpdateProject_AsOwner_ShouldReturnNoContent()
        {
            // Arrange
            EnsureTestProjectId(); 
            var updateDto = new UpdateProjectDto
            {
                Name = "Updated Project Name",
                Description = "Updated description.",
                IsTeamSearchActive = false, 
                ProjectTechnologies = new List<Guid> { _techId1 }
            };
            var content = new StringContent(JsonConvert.SerializeObject(updateDto), Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", _projectOwnerId.ToString());

            // Act
            var response = await _client.PutAsync($"/api/projects/{_testProjectId}", content);

            // Assert
            await EnsureSuccessOrOutputError(response); // 💡 Діагностика 500
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Перевірка, що зміни застосовані
            var detailsResponse = await _client.GetAsync($"/api/projects/{_testProjectId}");
            // Перевіряємо успішність, щоб уникнути NullReferenceException
            detailsResponse.EnsureSuccessStatusCode(); 
            var details = JsonConvert.DeserializeObject<ProjectDetailsDto>(await detailsResponse.Content.ReadAsStringAsync());
            Assert.Equal("Updated Project Name", details.Name);
            Assert.False(details.IsTeamSearchActive);
        }
        
        [Fact]
        public async Task Test06_UpdateProject_AsUnauthorizedCandidate_ShouldReturnForbidden()
        {
            // Arrange
            EnsureTestProjectId(); 
            var updateDto = new UpdateProjectDto { Name = "Fail Update", Description = "Test", ProjectTechnologies = new List<Guid>() };
            var content = new StringContent(JsonConvert.SerializeObject(updateDto), Encoding.UTF8, "application/json");
            
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", _candidateUserId.ToString());

            // Act
            var response = await _client.PutAsync($"/api/projects/{_testProjectId}", content);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // --- Сценарії керування членами ---

        [Fact]
        public async Task Test07_AddMember_AsOwner_ShouldReturnNoContent()
        {
            // Arrange
            EnsureTestProjectId(); 
            var addDto = new AddMemberDto
            {
                CandidateId = _projectMemberId, // ID члена, якого додаємо
                Role = "Frontend Dev"
            };
            var content = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", _projectOwnerId.ToString());

            // Act
            var response = await _client.PostAsync($"/api/projects/{_testProjectId}/members", content);

            // Assert
            await EnsureSuccessOrOutputError(response); // 💡 Діагностика 500
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Перевірка, що член тепер має доступ 
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", _projectMemberId.ToString());
            var detailsResponse = await _client.GetAsync($"/api/projects/{_testProjectId}");
            // Перевіряємо успішність, щоб уникнути NullReferenceException
            detailsResponse.EnsureSuccessStatusCode(); 
            
            var details = JsonConvert.DeserializeObject<ProjectDetailsDto>(await detailsResponse.Content.ReadAsStringAsync());
            Assert.Contains(details.ProjectMembers, m => m.CandidateId == _projectMemberId && m.Role == "Frontend Dev");
        }

        [Fact]
        public async Task Test08_RemoveMember_AsOwner_ShouldReturnNoContent()
        {
            // Arrange
            EnsureTestProjectId(); 
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", _projectOwnerId.ToString());

            // Act
            var response = await _client.DeleteAsync($"/api/projects/{_testProjectId}/members/{_projectMemberId}");

            // Assert
            await EnsureSuccessOrOutputError(response); // 💡 Діагностика 500
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Перевірка, що член більше не має доступу
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", _projectMemberId.ToString());
            var detailsResponse = await _client.GetAsync($"/api/projects/{_testProjectId}");
            Assert.Equal(HttpStatusCode.Forbidden, detailsResponse.StatusCode);
        }

        [Fact]
        public async Task Test09_RemoveMember_TryingToRemoveOwner_ShouldReturnBadRequest()
        {
            // Arrange
            EnsureTestProjectId(); 
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", _projectOwnerId.ToString());

            // Act
            var response = await _client.DeleteAsync($"/api/projects/{_testProjectId}/members/{_projectOwnerId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Test10_RemoveMember_AsUnauthorizedCandidate_ShouldReturnForbidden()
        {
            // Arrange
            EnsureTestProjectId(); 
            
            // Спочатку додамо члена назад, якщо його було видалено
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InternHubDbContext>();
            
            // Перевіряємо, чи існує зв'язок ProjectMember
            // 💡 Виправлення: Уникаємо повторного додавання, якщо він існує, для чистоти тесту.
            if (!context.ProjectMembers.Any(m => m.ProjectId == _testProjectId && m.CandidateId == _projectMemberId))
            {
                 // Додаємо члена через API (використовуючи власника)
                var addDto = new AddMemberDto { CandidateId = _projectMemberId, Role = "Test Role" };
                var content = new StringContent(JsonConvert.SerializeObject(addDto), Encoding.UTF8, "application/json");
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Add("X-Test-UserId", _projectOwnerId.ToString());
                
                var addResponse = await _client.PostAsync($"/api/projects/{_testProjectId}/members", content);
                addResponse.EnsureSuccessStatusCode(); // Якщо тут 500, проблема у Test07
            }
           
            // Act: Видалення від імені не-власника (неавторизований кандидат)
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", _candidateUserId.ToString());
            var response = await _client.DeleteAsync($"/api/projects/{_testProjectId}/members/{_projectMemberId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}