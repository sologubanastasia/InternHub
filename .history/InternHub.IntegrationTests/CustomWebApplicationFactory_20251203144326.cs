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
using System.Collections.Generic;
using Moq; // ДОДАНО
using InternHub.Application.Services.Team; // ДОДАНО

namespace InternHub.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        // Властивість для доступу до Mock-об'єкта ITeamService у тестах
        public Mock<ITeamService> TeamServiceMock { get; private set; } 

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddJsonFile("appsettings.Development.json", optional: false);
            });

            builder.ConfigureServices(services =>
            {
                // Видалення існуючого DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<InternHubDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                var sp = services.BuildServiceProvider();
                var config = sp.GetRequiredService<IConfiguration>();
                var connectionString = config.GetConnectionString("DefaultConnection");

                // Додавання DbContext з використанням тестового (або реального) підключення
                services.AddDbContext<InternHubDbContext>(options =>
                    options.UseNpgsql(connectionString));

                // --- КОНФІГУРАЦІЯ MOQ ---
                // 1. Ініціалізація Moq та заміна реального сервісу на імітований
                TeamServiceMock = new Mock<ITeamService>();
                
                var teamServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ITeamService));

                if (teamServiceDescriptor != null)
                {
                    services.Remove(teamServiceDescriptor);
                }
                
                // Додаємо імітований сервіс як Singleton
                services.AddSingleton(TeamServiceMock.Object);
                // -------------------------

                // Налаштування тестової автентифікації
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                });

                // Створення бази даних та посів даних
                var sp2 = services.BuildServiceProvider();
                using var scope = sp2.CreateScope();
                var scopedServices = scope.ServiceProvider;

                var db = scopedServices.GetRequiredService<InternHubDbContext>();
                var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                // Очищення та створення бази даних
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
            // --- ІСНУЮЧІ ОСНОВНІ КОРИСТУВАЧІ ---
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

            // Створення сутності Candidate для базового користувача
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
            
            // --- ДОДАТКОВІ КОРИСТУВАЧІ ТА ПОСІВ ДЛЯ ТЕСТІВ КОНТРОЛЕРА ПРОЕКТІВ ---
            
            var ownerCandidateId = TestData.OwnerCandidateId;
            var memberCandidateId = TestData.MemberCandidateId;
            var nonMemberCandidateId = TestData.NonMemberCandidateId;

            // 1. Власник проекту (Owner)
            var ownerUser = new ApplicationUser { Id = ownerCandidateId, UserName = "owner@test.com", Email = "owner@test.com", EmailConfirmed = true, Name = "Owner", Surname = "User" };
            if (await userManager.FindByEmailAsync(ownerUser.Email) == null)
            {
                await userManager.CreateAsync(ownerUser, "Password123!");
                await userManager.AddToRoleAsync(ownerUser, RoleConstants.Candidate);
                db.Candidates.Add(new Candidate { Id = ownerCandidateId, UserId = ownerCandidateId, Email = ownerUser.Email });
            }

            // 2. Член проекту (Member)
            var memberUser = new ApplicationUser { Id = memberCandidateId, UserName = "member@test.com", Email = "member@test.com", EmailConfirmed = true, Name = "Member", Surname = "User" };
            if (await userManager.FindByEmailAsync(memberUser.Email) == null)
            {
                await userManager.CreateAsync(memberUser, "Password123!");
                await userManager.AddToRoleAsync(memberUser, RoleConstants.Candidate);
                db.Candidates.Add(new Candidate { Id = memberCandidateId, UserId = memberCandidateId, Email = memberUser.Email });
            }

            // 3. Кандидат, що не є членом проекту (Non-member)
            var nonMemberUser = new ApplicationUser { Id = nonMemberCandidateId, UserName = "nonmember@test.com", Email = "nonmember@test.com", EmailConfirmed = true, Name = "NonMember", Surname = "User" };
            if (await userManager.FindByEmailAsync(nonMemberUser.Email) == null)
            {
                await userManager.CreateAsync(nonMemberUser, "Password123!");
                await userManager.AddToRoleAsync(nonMemberUser, RoleConstants.Candidate);
                db.Candidates.Add(new Candidate { Id = nonMemberCandidateId, UserId = nonMemberCandidateId, Email = nonMemberUser.Email });
            }

            // --- Посів Технологій ---
            db.Technologies.AddRange(
                new Technology { Id = TestData.ExistingTech1Id, Name = "C#" },
                new Technology { Id = TestData.ExistingTech2Id, Name = "Azure" }
            );

            // --- Посів Проектів ---
            
            // 1. ProjectIdForDetails
            db.Projects.Add(new Project
            {
                Id = TestData.ProjectIdForDetails,
                CandidateId = ownerCandidateId, // Власник
                Name = "Details Project",
                Description = "Project for testing details retrieval.",
                ProjectTechnologies = new List<ProjectTechnology>
                {
                    new ProjectTechnology { TechnologyId = TestData.ExistingTech1Id }
                },
                ProjectMembers = new List<ProjectMember>
                {
                    new ProjectMember { ProjectId = TestData.ProjectIdForDetails, CandidateId = ownerCandidateId, Role = "Owner" },
                    new ProjectMember { ProjectId = TestData.ProjectIdForDetails, CandidateId = memberCandidateId, Role = "Dev" } 
                }
            });

            // 2. ProjectIdForUpdate
            db.Projects.Add(new Project
            {
                Id = TestData.ProjectIdForUpdate,
                CandidateId = ownerCandidateId,
                Name = "Update Project",
                Description = "Project for testing updates.",
                ProjectTechnologies = new List<ProjectTechnology>
                {
                    new ProjectTechnology { TechnologyId = TestData.ExistingTech2Id }
                },
                ProjectMembers = new List<ProjectMember>
                {
                    new ProjectMember { ProjectId = TestData.ProjectIdForUpdate, CandidateId = ownerCandidateId, Role = "Owner" }
                }
            });

            // 3. ProjectIdForMemberRemoval
            db.Projects.Add(new Project
            {
                Id = TestData.ProjectIdForMemberRemoval,
                CandidateId = ownerCandidateId,
                Name = "Removal Project",
                Description = "Project for testing member removal.",
                ProjectMembers = new List<ProjectMember>
                {
                    new ProjectMember { ProjectId = TestData.ProjectIdForMemberRemoval, CandidateId = ownerCandidateId, Role = "Owner" },
                    new ProjectMember { ProjectId = TestData.ProjectIdForMemberRemoval, CandidateId = memberCandidateId, Role = "Junior Dev" }
                }
            });
            
            await db.SaveChangesAsync();
        }
    }
}