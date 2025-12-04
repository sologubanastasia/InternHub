using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InternHub.Domain.Entities;

namespace InternHub.Infrastructure.Configurations
{
    public class ProjectTechnologyConfiguration : IEntityTypeConfiguration<ProjectTechnology>
    {
        public void Configure(EntityTypeBuilder<ProjectTechnology> entity)
        {
            entity.HasKey(p => new { p.ProjectId, p.TechnologyId});

            entity.HasOne(p => p.Project)
                .WithMany(p => p.ProjectTechnologies)
                .HasForeignKey(p => p.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(pt => pt.Technology)
                .WithMany(t => t.ProjectTechnologies)
                .HasForeignKey(pt => pt.TechnologyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}