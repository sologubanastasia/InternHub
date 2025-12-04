using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InternHub.Infrastructure.Identity;
using InternHub.Domain;

public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> entity)
    {
        entity.HasKey(c => c.Id);

        entity.Property(c => c.Email)
            .HasMaxLength(100);

        entity.HasMany(a => a.Applications)
            .WithOne(c => c.Candidate)
            .HasForeignKey(c => c.CandidateId);     

        entity.HasOne<ApplicationUser>()   // ApplicationUser не в Domain
      .WithOne(u => u.Candidate)
      .HasForeignKey<Candidate>(c => c.UserId);
       
    }
}
  