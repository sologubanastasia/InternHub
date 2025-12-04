using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InternHub.Domain;

public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> entity)
    {
        entity.HasKey(a => a.Id);

        entity.Property(a => a.AppliedDate)
            .IsRequired();

        entity.Property(a => a.Status)
            .IsRequired();

        entity.HasOne(a => a.Candidate)
            .WithMany(c => c.Applications)
            .HasForeignKey(a => a.CandidateId)  

        entity.HasOne(j => j.Job)      
    }
}