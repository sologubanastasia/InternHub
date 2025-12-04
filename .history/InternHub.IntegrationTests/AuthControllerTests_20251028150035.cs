using InternHub.Application.DTO.Auth;
using System;
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
                Email = $"candidate_{Guid.NewGuid()}@test.com",
                Password = "Password123!",
                Name = "John",
                Surname = "Doe"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
            var content = await response.Content.ReadAsStringAsync();

            Assert.True(response.IsSuccessStatusCode, $"Failed: {response.StatusCode} - {content}");

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
                Email = $"company_{Guid.NewGuid()}@test.com",
                Password = "Password123!",
                Name = "Jane",
                CompanyName = "Test Company"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register/company", dto);
            var content = await response.Content.ReadAsStringAsync();

            Assert.True(response.IsSuccessStatusCode, $"Failed: {response.StatusCode} - {content}");

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
                Password = "TestPassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
            var content = await response.Content.ReadAsStringAsync();

            Assert.True(response.IsSuccessStatusCode, $"Failed: {response.StatusCode} - {content}");

            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(result);
            Assert.Equal(dto.Email, result!.Email);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }
    }
}
