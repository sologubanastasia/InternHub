using InternHub.Application.DTO.Candidate;
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
        }

        [Fact]
        public async Task GetProfile_ReturnsOk()
        {
            _client.De
            var response = await _client.GetAsync("/api/profile");
            response.EnsureSuccessStatusCode();

            var profile = await response.Content.ReadFromJsonAsync<CandidateResponseDto>();
            Assert.NotNull(profile);
            Assert.NotNull(profile.GitHubUrl); // можна додатково перевірити інші поля
        }

        [Fact]
        public async Task UpdateProfile_ReturnsNoContent()
        {
            var dto = new UpdateCandidateDto
            {
                GitHubUrl = "https://github.com/updated",
                Email = "updated@example.com",
                Telegram = "@updated_user"
            };

            var response = await _client.PutAsJsonAsync("/api/profile", dto);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
