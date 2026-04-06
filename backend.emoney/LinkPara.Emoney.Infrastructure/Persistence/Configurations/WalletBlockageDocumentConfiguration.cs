using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class WalletBlockageDocumentConfiguration : IEntityTypeConfiguration<WalletBlockageDocument>
{
    public void Configure(EntityTypeBuilder<WalletBlockageDocument> builder)
    {
        builder.Property(b => b.WalletId).IsRequired();
        builder.Property(b => b.DocumentId).IsRequired();
        builder.Property(t => t.DocumentTypeId).IsRequired();
        builder.Property(b => b.Description).IsRequired(false).HasMaxLength(1000);
        builder.Property(b => b.FileName).IsRequired().HasMaxLength(200);
    }
}

