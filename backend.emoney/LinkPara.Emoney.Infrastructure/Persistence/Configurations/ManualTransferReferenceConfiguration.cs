using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class ManualTransferReferenceConfiguration : IEntityTypeConfiguration<ManualTransferReference>
{
    public void Configure(EntityTypeBuilder<ManualTransferReference> builder)
    {
        builder.Property(s => s.TransactionId).IsRequired();
        builder.Property(s => s.DocumentTypeId).IsRequired();
        builder.Property(s => s.DocumentType).IsRequired();
        builder.Property(s => s.DocumentId).IsRequired();
        
        builder.HasIndex(s => s.TransactionId);
        builder.HasIndex(s => new { s.TransactionId, s.DocumentType });
    }
}