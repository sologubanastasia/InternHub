using System.Threading.Tasks;
using Xunit;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

// *** ВИПРАВЛЕННЯ: Додано using для WebApplicationFactoryClientOptions ***
using Microsoft.AspNetCore.Mvc.Testing; 

// Припускаємо, що ці DTO та enum існують
using InternHub.Application.DTO.Team; 
using InternHub.Domain.Entities; 
using InternHub.Infrastructure; // Для доступу до InternHubDbContext

namespace InternHub.IntegrationTests.Controllers
{
    [Collection("Sequential")]
    // Клас для перевірки TeamController з реальною базою даних
    public class TeamControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        
        // ID користувачів/проектів, які ми використовуємо для перевірки
        private static readonly Guid OwnerCandidateId = TestData.OwnerCandidateId;
        private static readonly Guid NonMemberCandidateId = TestData.NonMemberCandidateId;
        private static readonly Guid ProjectIdForDetails = TestData.ProjectIdForDetails;
        private static readonly Guid ProjectIdForUpdate = TestData.ProjectIdForUpdate;

        // ВИПРАВЛЕННЯ CS1520: Змінено назву конструктора, щоб вона відповідала назві класу (TeamControllerTests)
        public TeamControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private HttpClient GetClientWithUser(Guid userId, string role = RoleConstants.Candidate)
        {
            // *** ВИПРАВЛЕНО: WebApplicationFactoryClientOptions тепер доступний ***
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
            client.DefaultRequestHeaders.Add("X-Test-Role", role);
            return client;
        }

        private StringContent GetContent(object dto)
        {
            var json = JsonSerializer.Serialize(dto);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        // --- POST /api/team/projects/{projectId}/apply (Створення запиту) ---

        [Fact]
        public async Task ApplyToProject_Succeeds_AndCreatesRequestInDb()
        {
            // Arrange
            var applicantUserId = NonMemberCandidateId;
            var projectId = ProjectIdForDetails;
            var client = GetClientWithUser(applicantUserId);
            var dto = new ApplyToProjectDto { Message = "Applying via integration test." };
            
            // 1. Отримати CandidateId
            using var dbContext = _factory.GetDbContext();
            var candidate = await dbContext.Candidates.SingleAsync(c => c.UserId == applicantUserId);
            var applicantCandidateId = candidate.Id;

            // 2. Перевіряємо, що запиту ще немає
            // *** ВИПРАВЛЕНО: ВИКОРИСТАННЯ dbContext.TeamRequests (згідно з вашим InternHubDbContext) ***
            var initialRequestsCount = await dbContext.TeamRequests 
                .CountAsync(r => r.ProjectId == projectId && r.CandidateId == applicantCandidateId);
            Assert.Equal(0, initialRequestsCount);

            // Act
            var response = await client.PostAsync($"/api/team/projects/{projectId}/apply", GetContent(dto));

            // Assert (HTTP)
            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Assert (DB state check)
            // *** ВИПРАВЛЕНО: ВИКОРИСТАННЯ dbContext.TeamRequests ***
            var createdRequest = await dbContext.TeamRequests 
                .Where(r => r.ProjectId == projectId && r.CandidateId == applicantCandidateId)
                .SingleOrDefaultAsync();

            Assert.NotNull(createdRequest);
            Assert.Equal("Applying via integration test.", createdRequest.Message);
            Assert.Equal(ApplicationStatus.Pending, createdRequest.Status);
        }

        // --- PUT /api/team/projects/{projectId}/requests/{requestId}/process (Обробка запиту - Прийняття) ---

        [Fact]
        public async Task ProcessRequestForProject_AcceptsRequest_AndAddsMemberToDb()
        {
            // Arrange
            var projectId = ProjectIdForUpdate;
            var ownerUserId = OwnerCandidateId;
            var newMemberUserId = NonMemberCandidateId; // Кандидат, що подає запит
            var role = "Tester";
            var dto = new ProcessRequestDto { Action = ApplicationStatus.Accepted, Role = role };
            
            // 1. Отримати CandidateId
            using var dbContext = _factory.GetDbContext();
            var newMemberCandidate = await dbContext.Candidates.SingleAsync(c => c.UserId == newMemberUserId);
            var newMemberCandidateId = newMemberCandidate.Id;

            // 2. Створити Pending запит у базі даних (як підготовчий етап)
            var requestId = Guid.NewGuid();
            // *** ВИПРАВЛЕНО: ВИКОРИСТАННЯ TeamRequest (відсутній тип ProjectRequest) ***
            // *** ВИПРАВЛЕНО: ВИКОРИСТАННЯ dbContext.TeamRequests ***
            dbContext.TeamRequests.Add(new TeamRequest 
            {
                Id = requestId,
                CandidateId = newMemberCandidateId,
                ProjectId = projectId,
                Status = ApplicationStatus.Pending,
                RequestDate = DateTime.UtcNow,
                // Додайте тут властивості, необхідні для TeamRequest, якщо вони є.
                // Наприклад: Message = "Test Request" 
            });
            await dbContext.SaveChangesAsync();
            
            // Перевіряємо, що його ще немає в членах
            var isAlreadyMember = await dbContext.ProjectMembers
                .AnyAsync(m => m.ProjectId == projectId && m.CandidateId == newMemberCandidateId);
            Assert.False(isAlreadyMember);
            
            var client = GetClientWithUser(ownerUserId);

            // Act
            var response = await client.PutAsync($"/api/team/projects/{projectId}/requests/{requestId}/process", GetContent(dto));

            // Assert (HTTP)
            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Assert (DB state check)
            // *** ВИПРАВЛЕНО: ВИКОРИСТАННЯ dbContext.TeamRequests ***
            var processedRequest = await dbContext.TeamRequests.FindAsync(requestId); 
            
            // 1. Перевірка статусу запиту
            // *** ВИПРАВЛЕННЯ: Додано перевірку на null, щоб уникнути CS8602 ***
            Assert.NotNull(processedRequest);
            Assert.Equal(ApplicationStatus.Accepted, processedRequest!.Status);
            
            // 2. Перевірка додавання члена
            var newMember = await dbContext.ProjectMembers
                .Where(m => m.ProjectId == projectId && m.CandidateId == newMemberCandidateId)
                .SingleOrDefaultAsync();
                
            Assert.NotNull(newMember);
            Assert.Equal(role, newMember.Role);
        }


        // --- PUT /api/team/projects/{projectId}/members/{memberId} (Видалення члена) --
    }
}