using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace InternHub.IntegrationTests
{
    public class CompanyControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CompanyControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "22222222-2222-2222-2222-222222222222");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Company");
        }

        [Fact]
        public async Task GetProfile_ReturnsOk()
        {
            var response = await _client.GetAsync($"/api/company/profile?userId=22222222-2222-2222-2222-222222222222");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task CreateJob_ReturnsNoContent()
        {
            var dto = new CreateJobDto
            {
                Title = "Integration Test Job",
                Requirements = "C#, SQL"
            };

            var response = await _client.PostAsJsonAsync($"/api/company/jobs?userId=22222222-2222-2222-2222-222222222222", dto);
            response.EnsureSuccessStatusCode();
        }
    }
}
