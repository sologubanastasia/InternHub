using InternHub.Application.DTO.Candidate;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace InternHub.IntegrationTests
{
    [Collection("Sequential")]
    public class CandidateControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CandidateControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "33333333-3333-3333-3333-333333333333");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Candidate");
        }

        [Fact]
        public async Task GetProfile_ReturnsOk()
        {
            var response = await _client.GetAsync($"/api/profile?userId=33333333-3333-3333-3333-333333333333");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task UpdateProfile_ReturnsNoContent()
        {
            var dto = new UpdateCandidateDto { GitHubUrl = "https://github.com/test" };
            var response = await _client.PutAsJsonAsync($"/api/profile?userId=33333333-3333-3333-3333-333333333333", dto);
            response.EnsureSuccessStatusCode();
        }
    }
}
