using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InternHub.Domain;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> entity)
    {
        entity.HasKey(c => c.Id);
        entity.Property(c => c.Name)
            .IsRequired().HasMaxLength(100);

        entity.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(100);

        entity.HasMany(c => c.Jobs)
            .WithOne(j => j.Company)
            .HasForeignKey(j => j.CompanyId);

        entity.HasMany(c => c.Documents)
            .WithOne(d => d.Company)
            .HasForeignKey(d => d.CompanyId);
    }
}