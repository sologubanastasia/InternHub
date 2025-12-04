using InternHub.Application.DTO.Job;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace InternHub.IntegrationTests
{
    [Collection("Sequential")]
    public class JobControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public JobControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "33333333-3333-3333-3333-333333333333"); 
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Candidate");
        }

        [Fact]
        public async Task GetAllJobs_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/jobs");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ApplyToJob_ReturnsNoContent()
        {
            var dto = new JobApplicationCreateDto { Message = "I'm interested" };
            var response = await _client.PostAsJsonAsync($"/api/jobs/44444444-4444-4444-4444-444444444444/apply?candidateUserId=33333333-3333-3333-3333-333333333333", dto);
            response.EnsureSuccessStatusCode();
        }
    }
}
