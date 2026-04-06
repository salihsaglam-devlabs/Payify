using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class MerchantDocumentConfiguration : IEntityTypeConfiguration<MerchantDocument>
{
    public void Configure(EntityTypeBuilder<MerchantDocument> builder)
    {
        builder.Property(b => b.DocumentId).IsRequired();
        builder.Property(b => b.DocumentTypeId).IsRequired();
        builder.Property(b => b.DocumentName).IsRequired().HasMaxLength(256);
        builder.Property(b => b.MerchantId).IsRequired();
    }
}