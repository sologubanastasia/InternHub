using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using InternHub.Application.Services.Project;
using InternHub.Application.DTO.Project;
using InternHub.API.Controllers; // Необхідно, якщо контролер знаходиться в окремій збірці

namespace InternHub.IntegrationTests.Controllers
{
    // Використовуємо IClassFixture для ініціалізації WebApplicationFactory
    public class ProjectControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly Mock<IProjectService> _projectServiceMock;

        // Тестовий ідентифікатор користувача (власника)
        private static readonly Guid TestOwnerId = new Guid("4b123456-7890-abcd-ef01-234567890123");

        public ProjectControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

            // 1. Створення Mock-об'єкта для IProjectService
            _projectServiceMock = new Mock<IProjectService>();

            // 2. Створення тестового клієнта, який замінює IProjectService моком
            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Видаляємо оригінальний IProjectService
                    var serviceDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IProjectService));

                    if (serviceDescriptor != null)
                    {
                        services.Remove(serviceDescriptor);
                    }

                    // Додаємо Mock-об'єкт як синглтон
                    services.AddSingleton(_projectServiceMock.Object);
                });
            })
            .CreateClient();

            // Встановлення заголовку для автентифікації тестового користувача
            _client.DefaultRequestHeaders.Add("X-Test-UserId", TestOwnerId.ToString());
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Candidate"); 
        }

        // ------------------------------------------------------------------
        // GET /api/projects/my
        // ------------------------------------------------------------------

        [Fact]
        public async Task GetProjects_ReturnsOk_WithProjectsList()
        {
            // Arrange
            var mockProjects = new List<ProjectListItemDto>
            {
                new ProjectListItemDto { Id = Guid.NewGuid(), Name = "Project A" },
                new ProjectListItemDto { Id = Guid.NewGuid(), Name = "Project B" }
            };
            
            // Налаштовуємо мок, щоб повертав дані при виклику GetProjectsByOwnerAsync з TestOwnerId
            _projectServiceMock.Setup(s => s.GetProjectsByOwnerAsync(TestOwnerId))
                .ReturnsAsync(mockProjects);

            // Act
            var response = await _client.GetAsync("/api/projects/my");

            // Assert
            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var projects = JsonConvert.DeserializeObject<List<ProjectListItemDto>>(responseString);

            Assert.NotNull(projects);
            Assert.Equal(2, projects.Count);
        }
        
        // ------------------------------------------------------------------
        // POST /api/projects
        // ------------------------------------------------------------------

        [Fact]
        public async Task CreateProject_ReturnsCreated_WithNewProjectId()
        {
            // Arrange
            var newProjectId = Guid.NewGuid();
            var createDto = new CreateProjectDto 
            { 
                Name = "New Project", 
                Description = "Test description" 
            };
            
            // Налаштовуємо мок, щоб повертав новий ID при створенні
            _projectServiceMock.Setup(s => s.CreateProjectAsync(TestOwnerId, It.IsAny<CreateProjectDto>()))
                .ReturnsAsync(newProjectId);

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(createDto),
                System.Text.Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/projects", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            // Перевіряємо заголовок Location
            Assert.Contains(newProjectId.ToString(), response.Headers.Location.ToString());
            
            // Перевіряємо, що метод сервісу був викликаний
            _projectServiceMock.Verify(s => s.CreateProjectAsync(
                TestOwnerId, 
                It.Is<CreateProjectDto>(dto => dto.Name == createDto.Name)), 
                Times.Once());
        }

        // ------------------------------------------------------------------
        // GET /api/projects/{projectId}
        // ------------------------------------------------------------------

        [Fact]
        public async Task GetProjectId_ReturnsOk_WithProjectDetails()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var mockDetails = new ProjectDetailsDto { Id = projectId, Name = "Detailed Project" };

            _projectServiceMock.Setup(s => s.GetProjetDetailsAsync(projectId, TestOwnerId))
                .ReturnsAsync(mockDetails);

            // Act
            var response = await _client.GetAsync($"/api/projects/{projectId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseString = await response.Content.ReadAsStringAsync();
            var details = JsonConvert.DeserializeObject<ProjectDetailsDto>(responseString);

            Assert.Equal(projectId, details.Id);
        }

        // ------------------------------------------------------------------
        // PUT /api/projects/{projectId}
        // ------------------------------------------------------------------

        [Fact]
        public async Task UpdateProject_ReturnsNoContent_OnSuccess()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var updateDto = new UpdateProjectDto { Name = "Updated Name" };
            
            // Налаштовуємо мок, що метод виконається без помилок (void або Task)
            _projectServiceMock.Setup(s => s.UpdateProjectAsync(
                projectId, 
                TestOwnerId, 
                It.IsAny<UpdateProjectDto>()))
                .Returns(Task.CompletedTask);

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(updateDto),
                System.Text.Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync($"/api/projects/{projectId}", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            
            // Перевіряємо, що метод сервісу був викликаний
            _projectServiceMock.Verify(s => s.UpdateProjectAsync(
                projectId, 
                TestOwnerId, 
                It.Is<UpdateProjectDto>(dto => dto.Name == updateDto.Name)), 
                Times.Once());
        }

        // ------------------------------------------------------------------
        // POST /api/projects/{projectId}/members
        // ------------------------------------------------------------------

        [Fact]
        public async Task AddMember_ReturnsNoContent_OnSuccess()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var memberDto = new AddMemberDto { CandidateId = Guid.NewGuid() };
            
            // Налаштовуємо мок, що метод виконається без помилок
            _projectServiceMock.Setup(s => s.AddMemberAsync(
                projectId, 
                TestOwnerId, 
                It.IsAny<AddMemberDto>()))
                .Returns(Task.CompletedTask);

            // Важливо: для запитів з кількома [FromBody] або змішаними параметрами
            // часто використовують комплексний об'єкт DTO, але з огляду на ваш код контролера
            // (де ви помилково маєте два [FromBody] - це призведе до помилки Bind), 
            // для тестування ми передаємо лише тіло DTO, припускаючи, що 
            // `projectId` береться з маршруту.
            // *Примітка: в реальному коді контролера потрібно виправити AddMember(Guid projectId, [FromBody] AddMemberDto dto).*
            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(memberDto),
                System.Text.Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync($"/api/projects/{projectId}/members", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            _projectServiceMock.Verify(s => s.AddMemberAsync(
                projectId, 
                TestOwnerId, 
                It.IsAny<AddMemberDto>()), 
                Times.Once());
        }

        // ------------------------------------------------------------------
        // DELETE /api/projects/{projectId}/members/{memberId}
        // ------------------------------------------------------------------

        [Fact]
        public async Task RemoveMember_ReturnsNoContent_OnSuccess()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            
            _projectServiceMock.Setup(s => s.RemoveMemberAsync(
                projectId, 
                TestOwnerId, 
                memberId))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _client.DeleteAsync($"/api/projects/{projectId}/members/{memberId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            _projectServiceMock.Verify(s => s.RemoveMemberAsync(
                projectId, 
                TestOwnerId, 
                memberId), 
                Times.Once());
        }
        
        // ------------------------------------------------------------------
        // Тест на автентифікацію/авторизацію (хоча це покривається [Authorize])
        // ------------------------------------------------------------------

        [Fact]
        public async Task GetProjects_ReturnsUnauthorized_WhenNoAuthHeader()
        {
            // Arrange: Створюємо новий клієнт без встановленого заголовка X-Test-UserId
            using var unauthenticatedClient = _factory.CreateClient();

            // Act
            var response = await unauthenticatedClient.GetAsync("/api/projects/my");

            // Assert
            // Очікується 401 Unauthorized, оскільки контролер позначений як [Authorize]
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}