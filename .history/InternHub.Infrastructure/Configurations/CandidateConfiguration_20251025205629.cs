using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InternHub.Domain;

public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(IEntityTypeBuilder<Candidate> entity)
    {
        entity.HasKey(c => c.Id);

        entity.HasOne(c => c.User)
            .WithOne(u => u.User)
            .HasForeignKey(u => u.UserId);

        entity.HasMany(a => a.Applications)
            .WithOne(c => c.Candidate)
            .HasForeignKey(c => c.CandidateId);        
    }
}
  