using InternHub.Application.DTO.Auth;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace InternHub.IntegrationTests
{
    [Collection("Sequential")]
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RegisterCandidate_ReturnsOk()
        {
            var dto = new RegisterCandidateDto
            {
                Email = $"candidate_{Guid.NewGuid()}@test.com",
                Password = "Password123!",
                Name = "Test",
                Surname = "User"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
            response.EnsureSuccessStatusCode();
        }

       [Fact]
public async Task RegisterCompany_ReturnsOk()
{
    var dto = new RegisterCompanyDto
    {
        Email = $"test_{Guid.NewGuid()}@company.com",
        Password = "StrongP@ss123",
        Name = "John Doe",
        CompanyName = "Test Company"
    };

    var response = await _client.PostAsJsonAsync("/api/auth/register-company", dto);
    var body = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Response: {body}");
    
    response.EnsureSuccessStatusCode(); // should now pass
}


        [Fact]
        public async Task Login_ReturnsOk()
        {
            var dto = new LoginDto
            {
                Email = "candidate@test.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
            response.EnsureSuccessStatusCode();
        }
    }
}
