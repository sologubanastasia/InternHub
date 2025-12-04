using Microsoft.AspNetCore.Identity;                     
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore;          
using InternHub.Domain.Entities;                      


namespace InternHub.Infrastructure
{
    public class InternHubDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public InternHubDbContext(DbContextOptions<InternHubDbContext> options)
            : base(options)
        {

        }
        
        public InternHubDbContext() : base(
            new DbContextOptionsBuilder<InternHubDbContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=InternHubDb;Username=postgres;Password=password")
                .Options)
        { }
        public DbSet<Candidate> Candidates { get; set; } = null!;
        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<Application> Applications { get; set; } = null!;
        public DbSet<CompanyDocument> CompanyDocuments { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Technology> Technologies { get; set; } = null!;
        public DbSet<ProjectTechnology> ProjectTechnologies { get; set; } = null!;
        public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
        public DbSet<TeamRequest> TeamRequests { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(InternHubDbContext).Assembly);
        }
    }
}
