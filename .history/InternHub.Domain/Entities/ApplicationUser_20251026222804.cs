using Microsoft.AspNetCore.Identity;

namespace InternHub.Domain.Entities;
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public Candidate? Candidate { get; set; }
        public Company? Company { get; set; }
    }
}
