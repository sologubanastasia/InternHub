using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InternHub.Domain;

public class CompanyDocumentConfiguration : IEntityTypeConfiguration<CompanyDocument>
{
    public void Configure(EntityTypeBuilder<CompanyDocument> entity)
    {
        entity.HasKey(c => c.Id);

        entity.Property(c => c.FileName)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(c => c.FileUrl)
            .IsRequired()
            .HasMaxLength(250);

        entity.Property(c => c.UploadDate)
            .IsRequired();

        entity.HasOne(d => d.Company)
            .WithMany(c => c.Documents)
            .HasForeignKey(d => d.CompanyId)    
    }
}
  