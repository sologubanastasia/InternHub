using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Testing; 
using InternHub.Application.DTO.Team; 
using InternHub.Domain.Entities; 
using InternHub.Infrastructure; 
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Net;
using Xunit;

namespace InternHub.IntegrationTests.Controllers
{
    [Collection("Sequential")]
    public class TeamControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        
        private static readonly Guid OwnerCandidateId = TestData.OwnerCandidateId;
        private static readonly Guid NonMemberCandidateId = TestData.NonMemberCandidateId;
        private static readonly Guid ProjectIdForDetails = TestData.ProjectIdForDetails;
        private static readonly Guid ProjectIdForUpdate = TestData.ProjectIdForUpdate;

        public TeamControllerTests(CustomWebApplicationFactory factory)
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

        private StringContent GetContent(object dto)
        {
            var json = JsonSerializer.Serialize(dto);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        [Fact]
        public async Task ApplyToProject_Succeeds_AndCreatesRequestInDb()
        {
            var applicantUserId = NonMemberCandidateId;
            var projectId = ProjectIdForDetails;
            var client = GetClientWithUser(applicantUserId);
            var dto = new ApplyToProjectDto { Message = "Applying via integration test." };
            
            using var dbContext = _factory.GetDbContext();
            var candidate = await dbContext.Candidates.SingleAsync(c => c.UserId == applicantUserId);
            var applicantCandidateId = candidate.Id;

            var initialRequestsCount = await dbContext.TeamRequests 
                .CountAsync(r => r.ProjectId == projectId && r.CandidateId == applicantCandidateId);
            Assert.Equal(0, initialRequestsCount);

            var response = await client.PostAsync($"/api/team/projects/{projectId}/apply", GetContent(dto));

            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var createdRequest = await dbContext.TeamRequests 
                .Where(r => r.ProjectId == projectId && r.CandidateId == applicantCandidateId)
                .SingleOrDefaultAsync();

            Assert.NotNull(createdRequest);
            Assert.Equal("Applying via integration test.", createdRequest.Message);
            Assert.Equal(ApplicationStatus.Pending, createdRequest.Status);
        }

        [Fact]
        public async Task ProcessRequestForProject_AcceptsRequest_AndAddsMemberToDb()
        {
            var projectId = ProjectIdForUpdate;
            var ownerUserId = OwnerCandidateId;
            var newMemberUserId = NonMemberCandidateId; 
            var role = "Tester";
            var dto = new ProcessRequestDto { Action = ApplicationStatus.Accepted, Role = role };
            
            using var setupDbContext = _factory.GetDbContext(); 
            var newMemberCandidate = await setupDbContext.Candidates.SingleAsync(c => c.UserId == newMemberUserId);
            var newMemberCandidateId = newMemberCandidate.Id;

            var requestId = Guid.NewGuid();
            setupDbContext.TeamRequests.Add(new TeamRequest 
            {
                Id = requestId,
                CandidateId = newMemberCandidateId,
                ProjectId = projectId,
                Status = ApplicationStatus.Pending,
                RequestDate = DateTime.UtcNow,
            });
            await setupDbContext.SaveChangesAsync();
            
            var isAlreadyMember = await setupDbContext.ProjectMembers
                .AnyAsync(m => m.ProjectId == projectId && m.CandidateId == newMemberCandidateId);
            Assert.False(isAlreadyMember);
            
            var client = GetClientWithUser(ownerUserId);

            var response = await client.PutAsync($"/api/team/projects/{projectId}/requests/{requestId}/process", GetContent(dto));

            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            using var verificationDbContext = _factory.GetDbContext(); 
            var processedRequest = await verificationDbContext.TeamRequests.FindAsync(requestId); 
            
            Assert.NotNull(processedRequest);
            Assert.Equal(ApplicationStatus.Accepted, processedRequest!.Status);
            
            var newMember = await verificationDbContext.ProjectMembers
                .Where(m => m.ProjectId == projectId && m.CandidateId == newMemberCandidateId)
                .SingleOrDefaultAsync();
                
            Assert.NotNull(newMember);
            Assert.Equal(role, newMember.Role);
        }
    }
}