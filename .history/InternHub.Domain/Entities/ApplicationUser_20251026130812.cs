using Microsoft.AspNetCore.Identity;

namespace InternHub.Domain
{
    public class ApplicationApplicationUser : IdentityApplicationUser<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public Candidate? Candidate { get; set; }
        public Company? Company { get; set; }
    }
}
