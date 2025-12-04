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
                configBuilder.AddJsonFile("appsettings.Development.json", optional: false);
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<InternHubDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                var sp = services.BuildServiceProvider();
                var config = sp.GetRequiredService<IConfiguration>();
                var connectionString = config.GetConnectionString("DefaultConnection");

                services.AddDbContext<InternHubDbContext>(options =>
                    options.UseNpgsql(connectionString));

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

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

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

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
            var candidateUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var candidate = new ApplicationUser
            {
                Id = candidateUserId,
                UserName = "candidate@test.com",
                Email = "candidate@test.com",
                Name = "John",
                Surname = "Doe",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(candidate.Email) == null)
            {
                await userManager.CreateAsync(candidate, "Password123!");
                await userManager.AddToRoleAsync(candidate, RoleConstants.Candidate);
            }

            var companyUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var companyUser = new ApplicationUser
            {
                Id = companyUserId,
                UserName = "company@test.com",
                Email = "company@test.com",
                Name = "Jane",
                Surname = "Smith",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(companyUser.Email) == null)
            {
                await userManager.CreateAsync(companyUser, "Password123!");
                await userManager.AddToRoleAsync(companyUser, RoleConstants.Company);
            }

            var adminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var admin = new ApplicationUser
            {
                Id = adminUserId,
                UserName = "admin@test.com",
                Email = "admin@test.com",
                Name = "Admin",
                Surname = "User",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(admin.Email) == null)
            {
                await userManager.CreateAsync(admin, "Password123!");
                await userManager.AddToRoleAsync(admin, RoleConstants.Admin);
            }

            if (!db.Candidates.Any(c => c.UserId == candidateUserId))
            {
                db.Candidates.Add(new Candidate
                {
                    Id = Guid.NewGuid(),
                    UserId = candidateUserId,
                    Email = candidate.Email,
                    GitHubUrl = "https://github.com/test"
                });
            }

            if (!db.Companies.Any())
            {
                db.Companies.Add(new Company
                {
                    Id = companyUserId,
                    UserId = companyUserId,
                    CompanyName = "Test Company",
                    Email = companyUser.Email,
                    Status = CompanyStatus.WaitingForAdminApproval
                });
            }

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
