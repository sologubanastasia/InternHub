using InternHub.API;
using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InternHub.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                // Завантажуємо appsettings.Development.json для отримання ConnectionString
                configBuilder.AddJsonFile("appsettings.Development.json", optional: false);
            });

            builder.ConfigureServices(services =>
            {
                // Видаляємо дескриптор оригінального DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<InternHubDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                var sp = services.BuildServiceProvider();
                var config = sp.GetRequiredService<IConfiguration>();
                var connectionString = config.GetConnectionString("DefaultConnection");

                // Додаємо DbContext, використовуючи рядок підключення з appsettings
                services.AddDbContext<InternHubDbContext>(options =>
                    options.UseNpgsql(connectionString));

                // Реєструємо тестовий обробник автентифікації
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                // Встановлюємо тестову схему як стандартну
                services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                });

                var sp2 = services.BuildServiceProvider();
                using var scope = sp2.CreateScope();
                var scopedServices = scope.ServiceProvider;

                var db = scopedServices.GetRequiredService<InternHubDbContext>();
                var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                // Скидаємо та створюємо базу даних для чистого тесту
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // Посів початкових даних
                SeedRolesAsync(roleManager).GetAwaiter().GetResult();
                SeedTestDataAsync(userManager, db).GetAwaiter().GetResult();
            });
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            var roles = new[] { RoleConstants.Admin, RoleConstants.Company, RoleConstants.Candidate };
            foreach (var role in roles)
            {
                if (await roleManager.FindByNameAsync(role) == null)
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        private static async Task SeedTestDataAsync(UserManager<ApplicationUser> userManager, InternHubDbContext db)
        {
            // ID користувачів, які використовуються у всіх тестах
            var adminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var companyUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var candidateUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            
            // ID, що використовуються в ProjectControllerTests
            var projectOwnerId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var projectMemberId = Guid.Parse("66666666-6666-6666-6666-666666666666");

            // --- ДОПОМІЖНА ФУНКЦІЯ: Забезпечує існування ApplicationUser ---
            async Task EnsureUserExists(Guid userId, string email, string role, string name, string surname)
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        Id = userId,
                        UserName = email,
                        Email = email,
                        Name = name,
                        Surname = surname,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(user, "Password123!");
                    await userManager.AddToRoleAsync(user, role);
                }
            }

            // --- 1. Створення ApplicationUser (та їх додавання до ролей) ---
            
            await EnsureUserExists(adminUserId, "admin@test.com", RoleConstants.Admin, "Admin", "User");
            await EnsureUserExists(companyUserId, "company@test.com", RoleConstants.Company, "Jane", "Smith");
            await EnsureUserExists(candidateUserId, "candidate@test.com", RoleConstants.Candidate, "John", "Doe");
            await EnsureUserExists(projectOwnerId, "owner@test.com", RoleConstants.Candidate, "Project", "Owner");
            await EnsureUserExists(projectMemberId, "member@test.com", RoleConstants.Candidate, "Project", "Member");

            // --- 2. Створення допоміжних профілів (Candidate, Company) ---

            // Candidate (333...3333)
            if (!db.Candidates.Any(c => c.UserId == candidateUserId))
            {
                db.Candidates.Add(new Candidate
                {
                    // Встановлюємо Id = UserId, щоб можна було використовувати ApplicationUser.Id
                    // для прямого пошуку профілю в тестах (усунення помилок 500).
                    Id = candidateUserId, 
                    UserId = candidateUserId,
                    Email = "candidate@test.com",
                    GitHubUrl = "https://github.com/test"
                });
            }

            // Project Owner Candidate (555...5555)
            if (!db.Candidates.Any(c => c.UserId == projectOwnerId))
            {
                db.Candidates.Add(new Candidate
                {
                    Id = projectOwnerId,
                    UserId = projectOwnerId,
                    Email = "owner@test.com"
                });
            }

            // Project Member Candidate (666...6666)
            if (!db.Candidates.Any(c => c.UserId == projectMemberId))
            {
                db.Candidates.Add(new Candidate
                {
                    Id = projectMemberId,
                    UserId = projectMemberId,
                    Email = "member@test.com"
                });
            }
            
            // Company (222...2222)
            if (!db.Companies.Any(c => c.UserId == companyUserId))
            {
                db.Companies.Add(new Company
                {
                    Id = companyUserId,
                    UserId = companyUserId,
                    CompanyName = "Test Company",
                    Email = "company@test.com",
                    Status = CompanyStatus.WaitingForAdminApproval
                });
            }

            // --- 3. Створення інших сутностей (Jobs) ---

            if (!db.Jobs.Any())
            {
                db.Jobs.Add(new Job
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Title = "Test Job",
                    Requirements = "C#, SQL, .NET",
                    CompanyId = companyUserId
                });
            }

            await db.SaveChangesAsync();
        }
    }
}