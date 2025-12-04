using Microsoft.AspNetCore.Identity;

namespace InternHub.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}   