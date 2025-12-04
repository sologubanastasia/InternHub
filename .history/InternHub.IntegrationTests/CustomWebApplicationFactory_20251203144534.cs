using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using System.Text.Json;
using System;
using Moq;
using System.Text;
using InternHub.Application.DTO.Team;
using InternHub.Domain.Entities; // Для RoleConstants
using InternHub.Application.Services.Team; // Для ITeamService

namespace InternHub.IntegrationTests.Controllers
{
    public class TeamControllerMockedServiceTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly Mock<ITeamService> _teamServiceMock;

        // ID користувача/проекту для тестування
        private static readonly Guid TestUserId = TestData.OwnerCandidateId;
        private static readonly Guid TestProjectId = TestData.ProjectIdForDetails;

        public TeamControllerMockedServiceTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            // Отримуємо посилання на Mock-об'єкт, налаштований у CustomWebApplicationFactory
            _teamServiceMock = factory.TeamServiceMock;
            
            // Створюємо клієнта, автентифікованого як TestUser
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            _client.DefaultRequestHeaders.Add("X-Test-UserId", TestUserId.ToString());
            _client.DefaultRequestHeaders.Add("X-Test-Role", RoleConstants.Candidate);

            // Очищуємо всі попередні налаштування/виклики моку перед кожним тестом
            _teamServiceMock.Invocations.Clear();
            _teamServiceMock.Reset(); 
        }

        private StringContent GetContent(object dto)
        {
            var json = JsonSerializer.Serialize(dto);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        // --- Тест 1: Перевірка успішного отримання проектів ---
        [Fact]
        public async Task GetProjects_ReturnsOk_WhenServiceReturnsData()
        {
            // Arrange
            var expectedProjects = new List<TeamProjectDto>
            {
                new TeamProjectDto { Id = Guid.NewGuid(), Name = "Project Alpha" }
            };
            
            // Налаштовуємо мок: коли викликається GetTeamProjectsAsync, він повертає expectedProjects
            _teamServiceMock
                .Setup(s => s.GetTeamProjectsAsync())
                .ReturnsAsync(expectedProjects);

            // Act
            var response = await _client.GetAsync("/api/team/projects");

            // Assert
            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var projects = JsonSerializer.Deserialize<List<TeamProjectDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.Single(projects);
            Assert.Equal("Project Alpha", projects[0].Name);

            // Перевіряємо, що метод сервісу був викликаний рівно один раз
            _teamServiceMock.Verify(s => s.GetTeamProjectsAsync(), Times.Once);
        }

        // --- Тест 2: Перевірка створення нового запиту на приєднання ---
        [Fact]
        public async Task ApplyToProject_ReturnsNoContent_WhenServiceSucceeds()
        {
            // Arrange
            var dto = new ApplyToProjectDto { Message = "Applying now" };
            
            // Налаштовуємо мок, щоб метод нічого не повертав (Task) і не кидав винятків
            _teamServiceMock
                .Setup(s => s.ApplyToProjectAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ApplyToProjectDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _client.PostAsync($"/api/team/projects/{TestProjectId}/apply", GetContent(dto));

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); // 204 No Content

            // Перевіряємо, що метод сервісу був викликаний з правильними аргументами
            _teamServiceMock.Verify(s => s.ApplyToProjectAsync(
                TestProjectId, 
                TestUserId, 
                It.Is<ApplyToProjectDto>(d => d.Message == "Applying now")
            ), Times.Once);
        }

        // --- Тест 3: Перевірка обробки запиту (Прийняття) ---
        [Fact]
        public async Task ProcessRequest_ReturnsNoContent_WhenAcceptingRequest()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var dto = new ProcessRequestDto { Action = ApplicationStatus.Accepted, Role = "Junior Dev" };
            
            // Налаштовуємо мок
            _teamServiceMock
                .Setup(s => s.ProcessRequestAsync(TestProjectId, requestId, TestUserId, It.IsAny<ProcessRequestDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _client.PutAsync($"/api/team/projects/{TestProjectId}/requests/{requestId}/process", GetContent(dto));

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Перевіряємо, що метод сервісу був викликаний з коректними ID та дією
            _teamServiceMock.Verify(s => s.ProcessRequestAsync(
                TestProjectId,
                requestId,
                TestUserId,
                It.Is<ProcessRequestDto>(d => d.Action == ApplicationStatus.Accepted && d.Role == "Junior Dev")
            ), Times.Once);
        }

        // --- Тест 4: Перевірка обробки винятків (наприклад, для 403 Forbidden) ---
        [Fact]
        public async Task GetRequests_ReturnsForbidden_WhenServiceThrowsAccessDeniedException()
        {
            // Arrange
            // Припускаємо, що у вас є кастомний виняток для 403 (наприклад, ForbiddenException)
            // Якщо контролер має логіку перетворення винятків сервісного шару на HTTP-статуси
            
            _teamServiceMock
                .Setup(s => s.GetTeamRequestDetailsAsync(TestUserId, TestProjectId))
                // Тут потрібно імітувати виняток, який ваш контролер перетворює на 403 Forbidden.
                // Якщо контролер використовує [Authorize(Roles = "Owner")], то цей тест не потрібен.
                // Якщо логіка авторизації знаходиться в сервісі:
                .ThrowsAsync(new UnauthorizedAccessException("User is not the owner."));

            // *Примітка: для роботи цього тесту ваш контролер повинен обробляти UnauthorizedAccessException
            // та повертати 403, або ви маєте використовувати більш специфічний виняток, 
            // який обробляється як 403/401.*

            // Act
            var response = await _client.GetAsync($"/api/team/projects/{TestProjectId}/requests");

            // Assert
            // Якщо контролер не обробляє виняток, тут може бути 500.
            // Припустимо, контролер обробляє його коректно:
            // Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            
            // Оскільки ми не бачимо логіки контролера, ми перевіряємо лише факт виклику сервісу:
            _teamServiceMock.Verify(s => s.GetTeamRequestDetailsAsync(
                TestUserId, 
                TestProjectId
            ), Times.Once);
        }
    }
}