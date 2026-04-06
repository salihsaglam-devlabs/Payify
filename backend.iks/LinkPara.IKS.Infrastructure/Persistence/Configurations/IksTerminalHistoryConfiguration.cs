using LinkPara.IKS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.IKS.Infrastructure.Persistence.Configurations;

public class IksTerminalHistoryConfiguration : IEntityTypeConfiguration<IksTerminalHistory>
{
    public void Configure(EntityTypeBuilder<IksTerminalHistory> builder)
    {
        builder.Property(s => s.MerchantId).IsRequired();
        builder.Property(s => s.VposId).IsRequired();
        builder.Property(s => s.ReferenceCode).IsRequired().HasMaxLength(12);
        builder.Property(b => b.NewData).HasMaxLength(1000);
        builder.Property(b => b.OldData).HasMaxLength(1000);
        builder.Property(b => b.ChangedField).HasMaxLength(100);
        builder.Property(s => s.ResponseCode).HasMaxLength(3);
        builder.Property(s => s.ResponseCodeExplanation).HasMaxLength(2000);
    }
}
