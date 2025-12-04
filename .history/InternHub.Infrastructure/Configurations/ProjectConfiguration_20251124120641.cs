using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InternHub.Domain.Entities;

namespace InternHub.Infrastructure.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> entity)
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(p => p.Description)
                .HasMaxLength(300);

            entity.HasMany(p => p.ProjectMembers)
                .WithOne(m => m.Project)
                .HasForeignKey(m => m.ProjectId);

            entity.HasMany(p => p.ProjectTechnologies)
                .WithOne(p => p.Project)
                .HasForeignKey(p => p.ProjectId);    

            entity.HasMany(p => p.TeamRequests)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId);   

            entity.HasOne(p => p.Candidate)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);     
        }
    }
}
   