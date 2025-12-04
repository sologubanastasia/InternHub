using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InternHub.Domain.Entities;

namespace InternHub.Infrastructure.Configurations
{
    public class TeamRequestConfiguration : IEntityTypeConfiguration<TeamRequest>
    {
        public void Configure(EntityTypeBuilder<TeamRequest> entity)
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestDate)
                .IsRequired();
            entity.Property(e => e.Status)
                .IsRequired();
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.TeamRequests)
                  .HasForeignKey(e => e.ProjectId);

            entity.HasOne(e => e.Candidate)
                  .WithMany(c => c.TeamRequests)
                  .HasForeignKey(e => e.CandidateId)
                  .OnDelete(DeleteBehavior.Restrict);          
        }
    }
}