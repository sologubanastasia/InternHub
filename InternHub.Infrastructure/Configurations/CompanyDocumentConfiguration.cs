using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InternHub.Domain.Entities;

namespace InternHub.Infrastructure.Configurations
{
    public class CompanyDocumentConfiguration : IEntityTypeConfiguration<CompanyDocument>
    {
        public void Configure(EntityTypeBuilder<CompanyDocument> entity)
        {
            entity.HasKey(d => d.Id);

            entity.Property(d => d.FileName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(d => d.FileUrl)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(d => d.UploadDate)
                .IsRequired();

            entity.HasOne(d => d.Company)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CompanyId);
        }
    }
}
