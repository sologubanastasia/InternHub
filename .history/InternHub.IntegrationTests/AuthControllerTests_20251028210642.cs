using InternHub.Application.DTO.Admin;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace InternHub.IntegrationTests
{
    public class AdminControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AdminControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "11111111-1111-1111-1111-111111111111");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/admin/users");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ApproveCompany_ReturnsNoContent()
        {
            var dto = new ApproveCompanyDto { Approve = true };
            var response = await _client.PutAsJsonAsync("/api/admin/companies/22222222-2222-2222-2222-222222222222/approve", dto);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContent()
        {
            var response = await _client.DeleteAsync("/api/admin/users/33333333-3333-3333-3333-333333333333");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetAllJobs_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/admin/jobs");
            response.EnsureSuccessStatusCode();
        }
    }
}
