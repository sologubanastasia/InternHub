using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using InternHub.Application.DTO.Project;
using System.Net;
using System.Collections.Generic;
using System.Text.Json;
using System;
using System.Text;
using System.Linq;

namespace InternHub.IntegrationTests.Controllers
{
    public class ProjectControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ProjectControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        // Допоміжний метод для створення HttpClient з кастомними ID/Role
        private HttpClient GetClientWithUser(Guid userId, string role = RoleConstants.Candidate)
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
            client.DefaultRequestHeaders.Add("X-Test-Role", role);
            return client;
        }

        // Допоміжний метод для створення HttpClient, який імітує відсутність автентифікації
        private HttpClient GetUnauthenticatedClient()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Add("X-Test-UserId", "");
            return client;
        }

        // Допоміжний метод для серіалізації DTO
        private StringContent GetContent(object dto)
        {
            var json = JsonSerializer.Serialize(dto);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        // --- GET /api/projects/my ---
        [Fact]
        public async Task GetProjects_ReturnsOk_ForOwnerOrMember()
        {
            // Arrange
            var client = GetClientWithUser(TestData.OwnerCandidateId);

            // Act
            var response = await client.GetAsync("/api/projects/my");

            // Assert
            response.EnsureSuccessStatusCode(); // Status 200 OK
            var content = await response.Content.ReadAsStringAsync();
            var projects = JsonSerializer.Deserialize<List<ProjectListItemDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // Очікуємо 3 посіяні проекти, де OwnerCandidateId є власником
            Assert.NotNull(projects);
            Assert.Equal(3, projects.Count);
        }

        [Fact]
        public async Task GetProjects_ReturnsUnauthorized_WhenNoAuthHeaders()
        {
            // Arrange: Клієнт без автентифікації
            var client = GetUnauthenticatedClient(); 

            // Act
            var response = await client.GetAsync("/api/projects/my");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); 
        }

        // --- POST /api/projects ---
        [Fact]
        public async Task CreateProject_ReturnsCreated_WithValidDto()
        {
            // Arrange
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var newProjectDto = new CreateProjectDto
            {
                Name = "New Test Project", // ВИПРАВЛЕНО: Title -> Name
                Description = "A project to test creation.",
                ProjectTechnologies = new List<Guid> { TestData.ExistingTech1Id }
            };

            // Act
            var response = await client.PostAsync("/api/projects", GetContent(newProjectDto));

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode); // 201 Created
            var projectIdString = await response.Content.ReadAsStringAsync();
            Guid projectId = Guid.Parse(projectIdString.Trim('"'));
            Assert.NotEqual(Guid.Empty, projectId);

            // Перевірка Location Header
            Assert.Contains($"/api/projects/{projectId}", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task CreateProject_ReturnsNotFound_ForNonExistentTechnology()
        {
            // Arrange
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var invalidProjectDto = new CreateProjectDto
            {
                Name = "Invalid Project", // ВИПРАВЛЕНО: Title -> Name
                Description = "Should fail due to technology.",
                ProjectTechnologies = new List<Guid> { TestData.NonExistentTechId }
            };

            // Act
            var response = await client.PostAsync("/api/projects", GetContent(invalidProjectDto));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); // 404
        }
        
        // --- GET /api/projects/{projectId} ---
        [Fact]
        public async Task GetProjectId_ReturnsOk_ForOwner()
        {
            // Arrange: Використовуємо власника
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var projectId = TestData.ProjectIdForDetails;

            // Act
            var response = await client.GetAsync($"/api/projects/{projectId}");

            // Assert
            response.EnsureSuccessStatusCode(); // 200 OK
            var content = await response.Content.ReadAsStringAsync();
            var project = JsonSerializer.Deserialize<ProjectDetailsDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.NotNull(project);
            Assert.Equal("Details Project", project.Name); // ВИПРАВЛЕНО: Title -> Name
            Assert.True(project.ProjectMembers.Count > 0); // ВИПРАВЛЕНО: Members -> ProjectMembers
        }

        [Fact]
        public async Task GetProjectId_ReturnsOk_ForMember()
        {
            // Arrange: Член, який не є власником, але є членом проекту
            var client = GetClientWithUser(TestData.MemberCandidateId);
            var projectId = TestData.ProjectIdForDetails;

            // Act
            var response = await client.GetAsync($"/api/projects/{projectId}");

            // Assert
            response.EnsureSuccessStatusCode(); // 200 OK
        }
        
        [Fact]
        public async Task GetProjectId_ReturnsForbidden_ForNonMember()
        {
            // Arrange: Користувач, який не є власником чи членом
            var client = GetClientWithUser(TestData.NonMemberCandidateId);
            var projectId = TestData.ProjectIdForDetails;

            // Act
            var response = await client.GetAsync($"/api/projects/{projectId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); 
        }

        // --- PUT /api/projects/{projectId} ---
        [Fact]
        public async Task UpdateProject_ReturnsNoContent_ForValidUpdate()
        {
            // Arrange
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var projectId = TestData.ProjectIdForUpdate;
            var updateDto = new UpdateProjectDto
            {
                Name = "Updated Title", // ВИПРАВЛЕНО: Title -> Name
                Description = "New Description",
                ProjectTechnologies = new List<Guid> { TestData.ExistingTech1Id, TestData.ExistingTech2Id }
            };

            // Act
            var response = await client.PutAsync($"/api/projects/{projectId}", GetContent(updateDto));

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); // 204
        }
        
        [Fact]
        public async Task UpdateProject_ReturnsForbidden_ForNonOwner()
        {
            // Arrange
            var client = GetClientWithUser(TestData.MemberCandidateId); // Член проекту, але не власник
            var projectId = TestData.ProjectIdForUpdate;
            var updateDto = new UpdateProjectDto { Name = "Forbidden Update" }; // ВИПРАВЛЕНО: Title -> Name

            // Act
            var response = await client.PutAsync($"/api/projects/{projectId}", GetContent(updateDto));

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); // 403
        }

        // --- POST /api/projects/{projectId}/members ---
        [Fact]
        public async Task AddMember_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange: Створюємо новий проект, щоб переконатися, що CandidateId ще не є членом.
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var createDto = new CreateProjectDto { Name = "ForMemberAdd", Description = "Test", ProjectTechnologies = new List<Guid>() }; // ВИПРАВЛЕНО: Title -> Name
            var createResponse = await client.PostAsync("/api/projects", GetContent(createDto));
            var projectId = Guid.Parse((await createResponse.Content.ReadAsStringAsync()).Trim('"'));

            var addMemberDto = new AddMemberDto { CandidateId = TestData.NonMemberCandidateId, Role = "Tester" }; // NonMemberCandidateId ще не член

            // Act
            var response = await client.PostAsync($"/api/projects/{projectId}/members", GetContent(addMemberDto));

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); // 204
        }

        [Fact]
        public async Task AddMember_ReturnsBadRequest_WhenCandidateIsAlreadyOwner()
        {
            // Arrange: Власник намагається додати себе (OwnerCandidateId)
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var projectId = TestData.ProjectIdForDetails;
            var addMemberDto = new AddMemberDto { CandidateId = TestData.OwnerCandidateId, Role = "Co-Owner" };

            // Act
            var response = await client.PostAsync($"/api/projects/{projectId}/members", GetContent(addMemberDto));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // 400
        }

        // --- DELETE /api/projects/{projectId}/members/{memberId} ---
        [Fact]
        public async Task RemoveMember_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange: Проект має члена з MemberCandidateId
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var projectId = TestData.ProjectIdForMemberRemoval;
            var memberIdToRemove = TestData.MemberCandidateId;
            
            // Act
            var response = await client.DeleteAsync($"/api/projects/{projectId}/members/{memberIdToRemove}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); // 204
        }

        [Fact]
        public async Task RemoveMember_ReturnsBadRequest_WhenTryingToRemoveOwner()
        {
            // Arrange
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var projectId = TestData.ProjectIdForMemberRemoval;
            var ownerId = TestData.OwnerCandidateId; 

            // Act
            var response = await client.DeleteAsync($"/api/projects/{projectId}/members/{ownerId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); 
        }
        
        [Fact]
        public async Task RemoveMember_ReturnsForbidden_WhenRequesterIsNotOwner()
        {
            // Arrange
            var client = GetClientWithUser(TestData.MemberCandidateId); // Член намагається видалити іншого члена
            var projectId = TestData.ProjectIdForMemberRemoval;
            var memberIdToRemove = TestData.MemberCandidateId; 

            // Act
            var response = await client.DeleteAsync($"/api/projects/{projectId}/members/{memberIdToRemove}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); 
        }
    }
}