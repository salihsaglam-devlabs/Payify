using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class ChargebackDocumentConfiguration : IEntityTypeConfiguration<ChargebackDocument>
{
    public void Configure(EntityTypeBuilder<ChargebackDocument> builder)
    {
        builder.Property(b => b.ChargebackId).IsRequired();
        builder.Property(b => b.TransactionId).IsRequired();
        builder.Property(b => b.DocumentId).IsRequired();
        builder.Property(t => t.DocumentTypeId).IsRequired();
        builder.Property(b => b.Description).IsRequired(false).HasMaxLength(500);
        builder.Property(b => b.FileName).IsRequired().HasMaxLength(200);
    }
}