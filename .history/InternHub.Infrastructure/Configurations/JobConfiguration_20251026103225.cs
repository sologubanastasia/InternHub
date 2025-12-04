using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InternHub.Domain;

public class JobConfiguration :IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> entity)
    {
        entity.HasKey(j => j.Id);

        entity.HasMany(j => j.Applications)
            .WithOne(a => a.Job)
            .HasForeignKey(a => a.JobId);

        entity.Property(j => j.Title)
            .IsRequired()
            .HasMaxLength(150);

        entity.Property(j => j.Requirements)
            .IsRequired()
            .HasMaxLength(1000);

            
    }
}