using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class SubMerchantDocumentConfiguration : IEntityTypeConfiguration<SubMerchantDocument>
{
    public void Configure(EntityTypeBuilder<SubMerchantDocument> builder)
    {
        builder.Property(b => b.DocumentId).IsRequired();
        builder.Property(b => b.DocumentTypeId).IsRequired();
        builder.Property(b => b.DocumentName).IsRequired().HasMaxLength(256);
        builder.Property(b => b.SubMerchantId).IsRequired();
    }
}