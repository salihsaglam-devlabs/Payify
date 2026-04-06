using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;

public class AgreementDocumentConfiguration : IEntityTypeConfiguration<AgreementDocument>
{ 
    public void Configure(EntityTypeBuilder<AgreementDocument> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(t => t.RecordStatus);
        builder.Property(t => t.LanguageCode).HasMaxLength(10).IsRequired();
        builder.Property(t => t.LastVersion).HasMaxLength(10).IsRequired();
        builder.Property(t => t.ProductType).IsRequired();
    }
}