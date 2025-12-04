namespace InternHub.API;
using InternHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;

public static class RoleInitializer
{
        public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            var roles = new[] {
                RoleConstants.Candidate,
                RoleConstants.Company,
                RoleConstants.Admin
                };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName)) 
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
                }
            }
        }
}
