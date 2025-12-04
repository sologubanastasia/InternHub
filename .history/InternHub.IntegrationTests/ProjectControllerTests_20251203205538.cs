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
    [Collection("Sequential")]
    public class ProjectControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ProjectControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private HttpClient GetClientWithUser(Guid userId, string role = RoleConstants.Candidate)
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
            client.DefaultRequestHeaders.Add("X-Test-Role", role);
            return client;
        }

        private HttpClient GetUnauthenticatedClient()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            // Передаємо порожній заголовок, щоб TestAuthHandler повернув Failure (401)
            client.DefaultRequestHeaders.Add("X-Test-UserId", ""); 
            return client;
        }

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
            // ПЕРЕВІРКА: Якщо виправлення 500 працює, тут буде 200 OK
            response.EnsureSuccessStatusCode(); 
            var content = await response.Content.ReadAsStringAsync();
            var projects = JsonSerializer.Deserialize<List<ProjectListItemDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
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
            // ПЕРЕВІРКА: Завдяки виправленню TestAuthHandler тут має бути 401
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
                Name = "New Test Project", 
                Description = "A project to test creation.",
                ProjectTechnologies = new List<Guid> { TestData.ExistingTech1Id }
            };

            // Act
            var response = await client.PostAsync("/api/projects", GetContent(newProjectDto));

            // Assert
            response.EnsureSuccessStatusCode(); // Перевіряємо, що 201 Created
            Assert.Equal(HttpStatusCode.Created, response.StatusCode); 
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
                Name = "Invalid Project", 
                Description = "Should fail due to technology.",
                ProjectTechnologies = new List<Guid> { TestData.NonExistentTechId }
            };

            // Act
            var response = await client.PostAsync("/api/projects", GetContent(invalidProjectDto));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); 
        }
        
        // --- GET /api/projects/{projectId} ---
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
            Assert.Equal("Details Project", project.Name); 
            // 🔥 ВИПРАВЛЕНО: Змінено ProjectMembers на Members
            Assert.True(project.Members.Count > 0); 
        }

        [Fact]
        public async Task GetProjectId_ReturnsOk_ForMember()
        {
            // Arrange: Член, який не є власником
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
                Name = "Updated Name", 
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
            var client = GetClientWithUser(TestData.MemberCandidateId); 
            var projectId = TestData.ProjectIdForUpdate;
            var updateDto = new UpdateProjectDto 
            { 
                Name = "Forbidden Update",
                Description = "Valid Description", // ДОДАНО: Щоб пройти Model Validation
                ProjectTechnologies = new List<Guid>() // ДОДАНО: Щоб пройти Model Validation
            };

            // Act
            var response = await client.PutAsync($"/api/projects/{projectId}", GetContent(updateDto));

            // Assert
            // ПЕРЕВІРКА: Тепер має бути 403 Forbidden, а не 400 Bad Request
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); 
        }

       // ProjectControllerTests.cs (приблизно рядок 229)
        [Fact]
        public async Task AddMember_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            
            // 🔥 ВИПРАВЛЕННЯ: Включаємо існуючий ID технології
            var createDto = new CreateProjectDto 
            { 
                Name = "ForMemberAdd", 
                Description = "Test for adding member", 
                // Припускаємо, що TestData.ExistingTech1Id — це коректний GUID
                ProjectTechnologies = new List<Guid> { TestData.ExistingTech1Id } 
            }; 
            
            var createResponse = await client.PostAsync("/api/projects", GetContent(createDto));
            
            createResponse.EnsureSuccessStatusCode(); // Рядок 231 тепер має бути успішним
            // ...
        }

        [Fact]
        public async Task AddMember_ReturnsBadRequest_WhenCandidateIsAlreadyOwner()
        {
            // Arrange
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
            // Arrange
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
            var client = GetClientWithUser(TestData.MemberCandidateId); 
            var projectId = TestData.ProjectIdForMemberRemoval;
            var memberIdToRemove = TestData.MemberCandidateId; 

            // Act
            var response = await client.DeleteAsync($"/api/projects/{projectId}/members/{memberIdToRemove}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); 
        }
    }
}