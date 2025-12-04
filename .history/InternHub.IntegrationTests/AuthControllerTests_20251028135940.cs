using InternHub.Application.DTO.Auth;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace InternHub.IntegrationTests
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RegisterCandidate_ReturnsAuthResponse()
        {
            var dto = new RegisterCandidateDto
            {
                Email = "candidate@test.com",
                Password = "Password123!",
                Name = "John",
                Surname = "Doe"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(result);
            Assert.Equal(dto.Email, result!.Email);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }

        [Fact]
        public async Task RegisterCompany_ReturnsAuthResponse()
        {
            var dto = new RegisterCompanyDto
            {
                Email = "company@test.com",
                Password = "Password123!",
                Name = "Jane",
                CompanyName = "Test Company"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register/company", dto);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(result);
            Assert.Equal(dto.Email, result!.Email);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }

        [Fact]
        public async Task Login_ReturnsAuthResponse()
        {
            var dto = new LoginDto
            {
                Email = "candidate@test.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(result);
            Assert.Equal(dto.Email, result!.Email);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }
    }
}
