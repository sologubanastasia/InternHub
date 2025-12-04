using InternHub.Application.DTO.Project;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace InternHub.IntegrationTests
{
    [Collection("Sequential")]
    public class ProjectControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;
        private readonly Guid _ownerId = Utilities.Candidate1Id;
        private readonly Guid _candidateId = Utilities.CandidateId2;
        public ProjectControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private void AuthorizeCliet(Guid userId)
        {
            _client.DefaultRequestHeaders.Clear();
            var token = Utilities.GetCandidateToken(userId);
            _client.DefaultRequestHeaders.Add("Authorization",$"Bearer {token}");
        }

        [Fact]
        public async Task GetProjects_ReturnsOk_ForAutorizedUser()
        {
            AuthorizeClient(_ownerId);

            var response = await _client.GetAsync("api/projects/my");

            response.EnsureSuccessStatusCode();
            var projects = await response.Content.ReadFromJsonAsync<List<ProjectListItemDto>>();
            Assert.NotNull(projects);
        }

       [Fact]
        public async Task CreateProject_ReturnsCreated_WithValidData()
        {
            // Arrange
            AuthorizeClient(_ownerId);
            var newProject = new CreateProjectDto
            (
                Title: "New Test Project",
                Description: "Description for test project",
                ProjectTechnologies: new List<Guid> { Utilities.ExistingTechId } // Припускаємо, що цей ID існує
            );

            // Act
            var response = await _client.PostAsJsonAsync("api/projects", newProject);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode); // Status 201 Created
            
            // Перевіряємо, що повернутий результат є Guid (ID створеного проєкту)
            var projectId = await response.Content.ReadFromJsonAsync<Guid>();
            Assert.NotEqual(Guid.Empty, projectId);

            // Перевіряємо заголовок Location
            Assert.Contains($"/api/projects/{projectId}", response.Headers.Location?.ToString());
        }

        [Fact]
        public async Task CreateProject_ReturnsBadRequest_WithInvalidData()
        {
            // Arrange
            AuthorizeClient(_ownerId);
            // Title = null або порожнє, що викличе помилку валідації 400
            var invalidProject = new CreateProjectDto(
                Title: null!, 
                Description: "Missing title",
                ProjectTechnologies: new List<Guid>()
            );

            // Act
            var response = await _client.PostAsJsonAsync("api/projects", invalidProject);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Status 400
        }

        // --- 3. Тести GET: api/projects/{projectId} ---

        [Fact]
        public async Task GetProjectId_ReturnsOk_ForOwner()
        {
            // Arrange
            AuthorizeClient(_ownerId);
            var existingProjectId = Utilities.ExistingProjectIdForOwner1; // Припускаємо, що цей ID належить _ownerId

            // Act
            var response = await _client.GetAsync($"api/projects/{existingProjectId}");

            // Assert
            response.EnsureSuccessStatusCode(); // Status 200 OK
            var projectDetails = await response.Content.ReadFromJsonAsync<ProjectDetailsDto>();
            Assert.NotNull(projectDetails);
            Assert.Equal(existingProjectId, projectDetails.Id);
        }

        [Fact]
        public async Task GetProjectId_ReturnsForbidden_ForUnauthorizedAccess()
        {
            // Arrange
            AuthorizeClient(_nonOwnerId); // Користувач, який не є ані власником, ані членом
            var restrictedProjectId = Utilities.ExistingProjectIdForOwner1; 

            // Act
            var response = await _client.GetAsync($"api/projects/{restrictedProjectId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); // Status 403 Forbidden
        }

        // --- 4. Тести PUT: api/projects/{projectId} ---

        [Fact]
        public async Task UpdateProject_ReturnsNoContent_ForValidOwnerUpdate()
        {
            // Arrange
            AuthorizeClient(_ownerId);
            var projectIdToUpdate = Utilities.ExistingProjectIdForOwner1;
            var updateDto = new UpdateProjectDto
            (
                Title: "Updated Title",
                Description: "New Description"
            );

            // Act
            var response = await _client.PutAsJsonAsync($"api/projects/{projectIdToUpdate}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); // Status 204 NoContent
        }

        [Fact]
        public async Task UpdateProject_ReturnsForbidden_ForNonOwnerAttempt()
        {
            // Arrange
            AuthorizeClient(_nonOwnerId);
            var restrictedProjectId = Utilities.ExistingProjectIdForOwner1;
            var updateDto = new UpdateProjectDto(
                Title: "Trying to hack",
                Description: "Attempt"
            );

            // Act
            var response = await _client.PutAsJsonAsync($"api/projects/{restrictedProjectId}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); // Status 403 Forbidden
        }

        // --- 5. Тести POST: api/projects/{projectId}/members ---

        [Fact]
        public async Task AddMember_ReturnsNoContent_WhenOwnerAddsNewMember()
        {
            // Arrange
            AuthorizeClient(_ownerId);
            var projectId = Utilities.ExistingProjectIdForOwner1;
            var newMemberId = Utilities.Candidate3Id; // Припускаємо, що це ID, який ще не є членом проєкту
            var addMemberDto = new AddMemberDto(
                CandidateId: newMemberId,
                Role: "Developer"
            );

            // Act
            var response = await _client.PostAsJsonAsync($"api/projects/{projectId}/members", addMemberDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); // Status 204 NoContent
        }

        [Fact]
        public async Task AddMember_ReturnsForbidden_WhenNonOwnerAttemptsToAdd()
        {
            // Arrange
            AuthorizeClient(_nonOwnerId);
            var projectId = Utilities.ExistingProjectIdForOwner1;
            var newMemberId = Utilities.Candidate3Id;
            var addMemberDto = new AddMemberDto(
                CandidateId: newMemberId,
                Role: "Guest"
            );

            // Act
            var response = await _client.PostAsJsonAsync($"api/projects/{projectId}/members", addMemberDto);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); // Status 403 Forbidden
        }

        // --- 6. Тести DELETE: api/projects/{projectId}/members/{memberId} ---

        [Fact]
        public async Task RemoveMember_ReturnsNoContent_WhenOwnerRemovesMember()
        {
            // Arrange
            AuthorizeClient(_ownerId);
            var projectId = Utilities.ExistingProjectWithMemberId; // Проєкт з існуючим членом
            var memberToRemoveId = Utilities.ExistingMemberId; // ID члена проєкту

            // Act
            var response = await _client.DeleteAsync($"api/projects/{projectId}/members/{memberToRemoveId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); // Status 204 NoContent
        }

        [Fact]
        public async Task RemoveMember_ReturnsForbidden_WhenNonOwnerAttemptsToRemove()
        {
            // Arrange
            AuthorizeClient(_nonOwnerId);
            var projectId = Utilities.ExistingProjectWithMemberId;
            var memberToRemoveId = Utilities.ExistingMemberId;

            // Act
            var response = await _client.DeleteAsync($"api/projects/{projectId}/members/{memberToRemoveId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); // Status 403 Forbidden
        }
    }

}
