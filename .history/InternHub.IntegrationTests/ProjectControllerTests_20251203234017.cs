using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using InternHub.Application.DTO.Project;
using InternHub.API;
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

        private HttpClient GetClientWithUser(Guid userId, string role = "Candidate")
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
            client.DefaultRequestHeaders.Add("X-Test-Role", role);
            return client;
        }

        private HttpClient GetUnauthenticatedClient()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Add("X-Test-UserId", ""); 
            return client;
        }

        private StringContent GetContent(object dto)
        {
            var json = JsonSerializer.Serialize(dto);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
        
        [Fact]
        public async Task GetProjects_ReturnsOk_ForOwnerOrMember()
        {
            var client = GetClientWithUser(TestData.OwnerCandidateId);

            var response = await client.GetAsync("/api/projects/my");

            response.EnsureSuccessStatusCode(); 
            var content = await response.Content.ReadAsStringAsync();
            var projects = JsonSerializer.Deserialize<List<ProjectListItemDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.NotNull(projects);
            Assert.Equal(3, projects.Count);
        }

        [Fact]
        public async Task GetProjects_ReturnsUnauthorized_WhenNoAuthHeaders()
        {
            var client = GetUnauthenticatedClient(); 

            var response = await client.GetAsync("/api/projects/my");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); 
        }

        [Fact]
        public async Task CreateProject_ReturnsCreated_WithValidDto()
        {
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var newProjectDto = new CreateProjectDto
            {
                Name = "New Test Project", 
                Description = "A project to test creation.",
                ProjectTechnologies = new List<Guid> { TestData.ExistingTech1Id }
            };

            var response = await client.PostAsync("/api/projects", GetContent(newProjectDto));

            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.Created, response.StatusCode); 
            var projectIdString = await response.Content.ReadAsStringAsync();
            Guid projectId = Guid.Parse(projectIdString.Trim('"'));
            Assert.NotEqual(Guid.Empty, projectId);

            Assert.Contains($"/api/projects/{projectId}", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task CreateProject_ReturnsNotFound_ForNonExistentTechnology()
        {
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var invalidProjectDto = new CreateProjectDto
            {
                Name = "Invalid Project", 
                Description = "Should fail due to technology.",
                ProjectTechnologies = new List<Guid> { TestData.NonExistentTechId }
            };

            var response = await client.PostAsync("/api/projects", GetContent(invalidProjectDto));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); 
        }
        
        [Fact]
        public async Task GetProjectId_ReturnsOk_ForOwner()
        {
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var projectId = TestData.ProjectIdForDetails;

            var response = await client.GetAsync($"/api/projects/{projectId}");

            response.EnsureSuccessStatusCode(); 
            var content = await response.Content.ReadAsStringAsync();
            var project = JsonSerializer.Deserialize<ProjectDetailsDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.NotNull(project);
            Assert.Equal("Details Project", project.Name); 
            Assert.True(project.Members.Count > 0); 
        }

        [Fact]
        public async Task GetProjectId_ReturnsOk_ForMember()
        {
            var client = GetClientWithUser(TestData.MemberCandidateId);
            var projectId = TestData.ProjectIdForDetails;

            var response = await client.GetAsync($"/api/projects/{projectId}");

            response.EnsureSuccessStatusCode(); 
        }
        
        [Fact]
        public async Task GetProjectId_ReturnsForbidden_ForNonMember()
        {
            var client = GetClientWithUser(TestData.NonMemberCandidateId);
            var projectId = TestData.ProjectIdForDetails;

            var response = await client.GetAsync($"/api/projects/{projectId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); 
        }

        [Fact]
        public async Task UpdateProject_ReturnsNoContent_ForValidUpdate()
        {
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var projectId = TestData.ProjectIdForUpdate;
            var updateDto = new UpdateProjectDto
            {
                Name = "Updated Name", 
                Description = "New Description",
                ProjectTechnologies = new List<Guid> { TestData.ExistingTech1Id, TestData.ExistingTech2Id }
            };

            var response = await client.PutAsync($"/api/projects/{projectId}", GetContent(updateDto));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); 
        }
        
        [Fact]
        public async Task UpdateProject_ReturnsForbidden_ForNonOwner()
        {
            var client = GetClientWithUser(TestData.MemberCandidateId); 
            var projectId = TestData.ProjectIdForUpdate;
            var updateDto = new UpdateProjectDto 
            { 
                Name = "Forbidden Update",
                Description = "Valid Description", 
                ProjectTechnologies = new List<Guid>() 
            };

            var response = await client.PutAsync($"/api/projects/{projectId}", GetContent(updateDto));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); 
        }

        [Fact]
        public async Task AddMember_ReturnsNoContent_WhenSuccessful()
        {
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            
            var createDto = new CreateProjectDto 
            { 
                Name = "ForMemberAdd", 
                Description = "Test for adding member", 
                ProjectTechnologies = new List<Guid> { TestData.ExistingTech1Id } 
            }; 
            
            var createResponse = await client.PostAsync("/api/projects", GetContent(createDto));
            createResponse.EnsureSuccessStatusCode(); 
            var projectIdString = await createResponse.Content.ReadAsStringAsync();
            
            Guid projectId = Guid.Parse(projectIdString.Trim().Trim('"')); 

            var addMemberDto = new AddMemberDto { CandidateId = TestData.NonMemberCandidateId, Role = "Tester" }; 

            var response = await client.PostAsync($"/api/projects/{projectId}/members", GetContent(addMemberDto));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task AddMember_ReturnsBadRequest_WhenCandidateIsAlreadyOwner()
        {
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var projectId = TestData.ProjectIdForDetails;
            var addMemberDto = new AddMemberDto { CandidateId = TestData.OwnerCandidateId, Role = "Co-Owner" };

            var response = await client.PostAsync($"/api/projects/{projectId}/members", GetContent(addMemberDto));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); 
        }

        [Fact]
        public async Task RemoveMember_ReturnsNoContent_WhenSuccessful()
        {
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var projectId = TestData.ProjectIdForMemberRemoval;
            var memberIdToRemove = TestData.MemberCandidateId;
            
            var response = await client.DeleteAsync($"/api/projects/{projectId}/members/{memberIdToRemove}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); 
        }

        [Fact]
        public async Task RemoveMember_ReturnsBadRequest_WhenTryingToRemoveOwner()
        {
            var client = GetClientWithUser(TestData.OwnerCandidateId);
            var projectId = TestData.ProjectIdForMemberRemoval;
            var ownerId = TestData.OwnerCandidateId; 

            var response = await client.DeleteAsync($"/api/projects/{projectId}/members/{ownerId}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); 
        }
        
        [Fact]
        public async Task RemoveMember_ReturnsForbidden_WhenRequesterIsNotOwner()
        {
            var client = GetClientWithUser(TestData.MemberCandidateId); 
            var projectId = TestData.ProjectIdForMemberRemoval;
            var memberIdToRemove = TestData.MemberCandidateId; 

            var response = await client.DeleteAsync($"/api/projects/{projectId}/members/{memberIdToRemove}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); 
        }
    }
}