using System.Threading.Tasks;
using Xunit;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing; 
using InternHub.Application.DTO.Technology; 
using InternHub.Application.DTO.Pagination; 
using InternHub.IntegrationTests; // Для доступу до TestData та CustomWebApplicationFactory
using InternHub.Domain.Entities; // Для доступу до Technology
using InternHub.Infrastructure; // Для доступу до InternHubDbContext

namespace InternHub.IntegrationTests.Controllers
{
    // Використовуємо колекцію, щоб уникнути конфліктів стану бази даних між тестами
    [Collection("Sequential")]
    public class TechnologyControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        
        // ID користувачів для авторизації
        private static readonly Guid AuthorizedUserId = TestData.OwnerCandidateId;
        private static readonly Guid NonExistentUserId = Guid.NewGuid();
        
        // ID існуючих технологій
        private static readonly Guid ExistingTech1Id = TestData.ExistingTech1Id;
        private static readonly Guid NonExistentTechId = TestData.NonExistentTechId;

        public TechnologyControllerTests(CustomWebApplicationFactory factory)
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

        private HttpClient GetAnonymousClient()
        {
            return _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        private StringContent GetContent(object dto)
        {
            var json = JsonSerializer.Serialize(dto);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
        
        private T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        // --- POST /api/technology (Створення) ---

        [Fact]
        public async Task CreateTechnology_Succeeds_AndReturnsCreatedItem()
        {
            // Arrange
            var client = GetClientWithUser(AuthorizedUserId);
            var newTechName = "New Test Tech";
            var dto = new CreateTechnologyDto { Name = newTechName };
            
            // Act
            var response = await client.PostAsync("/api/technology", GetContent(dto));

            // Assert (HTTP)
            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Контролер повертає 200 OK з об'єктом

            var responseString = await response.Content.ReadAsStringAsync();
            var createdTechnology = Deserialize<TechnologyDto>(responseString);
            
            Assert.NotNull(createdTechnology);
            Assert.NotEqual(Guid.Empty, createdTechnology.Id);
            Assert.Equal(newTechName, createdTechnology.Name);

            // Assert (DB state check)
            using var dbContext = _factory.GetDbContext();
            var dbTechnology = await dbContext.Technologies.FindAsync(createdTechnology.Id);
            
            Assert.NotNull(dbTechnology);
            Assert.Equal(newTechName, dbTechnology.Name);
            
            // Cleanup (Видалення для наступних тестів)
            dbContext.Technologies.Remove(dbTechnology);
            await dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateTechnology_Unauthorized_Returns401()
        {
            // Arrange (Клієнт без заголовка авторизації)
            var client = GetAnonymousClient();
            var dto = new CreateTechnologyDto { Name = "Unauthorized Test" };
            
            // Act
            var response = await client.PostAsync("/api/technology", GetContent(dto));

            // Assert (HTTP)
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // --- GET /api/technology/{id} (Отримання за ID) ---

        [Fact]
        public async Task GetByIdTechnology_ExistingId_Succeeds_AndReturnsTechnology()
        {
            // Arrange
            var client = GetClientWithUser(AuthorizedUserId);
            
            // Act
            var response = await client.GetAsync($"/api/technology/{ExistingTech1Id}");

            // Assert (HTTP)
            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var technology = Deserialize<TechnologyDto>(responseString);
            
            Assert.NotNull(technology);
            Assert.Equal(ExistingTech1Id, technology.Id);
            Assert.Equal("C#", technology.Name);
        }

        [Fact]
        public async Task GetByIdTechnology_NonExistingId_Returns404()
        {
            var client = GetClientWithUser(AuthorizedUserId);
        
            var response = await client.GetAsync($"/api/technology/{NonExistentTechId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); 
        }

        [Fact]
        public async Task RemoveTechnology_Succeeds_AndRemovesFromDb()
        {
            var techIdToRemove = Guid.NewGuid();
            using (var dbContext = _factory.GetDbContext())
            {
                dbContext.Technologies.Add(new Technology { Id = techIdToRemove, Name = "To be deleted" });
                await dbContext.SaveChangesAsync();
            }
            
            var client = GetClientWithUser(AuthorizedUserId);
            
            var response = await client.DeleteAsync($"/api/technology/{techIdToRemove}");

            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); 

            using var verificationDbContext = _factory.GetDbContext();
            var dbTechnology = await verificationDbContext.Technologies.FindAsync(techIdToRemove);
            
            Assert.Null(dbTechnology);
        }
        
        [Fact]
        public async Task RemoveTechnology_NonExistingId_Returns404()
        {
            var client = GetClientWithUser(AuthorizedUserId);
            
            var response = await client.DeleteAsync($"/api/technology/{NonExistentTechId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); 
        }

        [Fact]
        public async Task GetTechnologyAsync_ValidQuery_ReturnsPagedResults()
        {
            
            var client = GetAnonymousClient(); 
            var query = new TechnologySearchQueryDto { PageNumber = 1, PageSize = 10, Name = "C#" };
            
            var response = await client.GetAsync($"/api/technology?PageNumber={query.PageNumber}&PageSize={query.PageSize}&Name={query.Name}");

            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var pagedResult = Deserialize<PagedResultDto<TechnologyDto>>(responseString);
            
            Assert.NotNull(pagedResult);
            Assert.Equal(1, pagedResult.TotalCount); 
            Assert.True(pagedResult.Items.All(t => t.Name.Contains("C#")));
        }

        [Fact]
        public async Task GetTechnologyAsync_InvalidQuery_Returns400BadRequest()
        {
            var client = GetAnonymousClient(); 
            var invalidQuery = new TechnologySearchQueryDto { PageNumber = 0, PageSize = 10 };
            
            var response = await client.GetAsync($"/api/technology?PageNumber={invalidQuery.PageNumber}&PageSize={invalidQuery.PageSize}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}