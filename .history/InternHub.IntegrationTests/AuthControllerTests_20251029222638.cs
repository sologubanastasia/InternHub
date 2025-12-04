using InternHub.Application.DTO.Auth;
using System;
using System.Net;
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
                Password = "Password123!",   // ✅ ≥6 символів, відповідає політиці пароля
                Name = "Test",
                Surname = "User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Candidate Register Response: {body}");

            // Assert
            response.EnsureSuccessStatusCode(); // 200 OK
        }

        [Fact]
        public async Task RegisterCompany_ReturnsOk()
        {
            // Arrange
            var dto = new RegisterCompanyDto
            {
                Email = $"company_{Guid.NewGuid()}@example.com",
                Password = "StrongP@ss123",    // ✅ ≥6 символів
                Name = "Valid Company Owner",
                CompanyName = "Integration Test Company" // ✅ не порожнє, ≤150 символів
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register/company", dto);
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Company Register Response: {body}");

            // Assert
            response.EnsureSuccessStatusCode(); // ✅ має бути 200 OK
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenUserDoesNotExist()
        {
            // Arrange — користувач не існує
            var dto = new LoginDto
            {
                Email = $"not_exists_{Guid.NewGuid()}@example.com",
                Password = "SomePassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Login Response: {body}");

            // Assert — має бути 400 або 401 (залежно від того, як кинута помилка)
            Assert.True(
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Unauthorized,
                $"Expected 400/401, got {(int)response.StatusCode}: {body}"
            );
        }

        [Fact]
        public async Task RegisterCompany_ReturnsBadRequest_WhenInvalidData()
        {
            // Arrange — спеціально зробимо невірні дані
            var dto = new RegisterCompanyDto
            {
                Email = "invalid-email", // ❌ неправильний формат
                Password = "123",        // ❌ занадто короткий
                Name = "",               // ❌ порожнє
                CompanyName = ""         // ❌ порожнє
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register/company", dto);
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Invalid Company Register Response: {body}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
