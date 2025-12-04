using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InternHub.Domain;

namespace InternHub.Infrastructure.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> entity)
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Website)
                .HasMaxLength(200);

            entity.Property(c => c.Description)
                .HasMaxLength(1000);

            entity.HasOne(c => c.User)
                .WithOne(u => u.Company)
                .HasForeignKey<Company>(c => c.UserId);

            entity.HasMany(c => c.Jobs)
                .WithOne(j => j.Company)
                .HasForeignKey(j => j.CompanyId);

            entity.HasMany(c => c.Documents)
                .WithOne(d => d.Company)
                .HasForeignKey(d => d.CompanyId);

            entity.HasOne<ApplicationUser>()
                .WithOne(u => u.Company)
                .HasForeignKey<Company>(c => c.UserId);
        }
    }
}
