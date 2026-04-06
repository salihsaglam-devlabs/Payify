using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class BulkTransferConfiguration : IEntityTypeConfiguration<BulkTransfer>
{
    public void Configure(EntityTypeBuilder<BulkTransfer> builder)
    {
        builder.Property(s => s.FileName).HasMaxLength(400);
        builder.Property(s => s.ActionUserName).HasMaxLength(150);
        builder.Property(s => s.SenderWalletNumber).HasMaxLength(50).IsRequired();
        builder.Property(s => s.ReferenceNumber).UseIdentityColumn();
    }
}
