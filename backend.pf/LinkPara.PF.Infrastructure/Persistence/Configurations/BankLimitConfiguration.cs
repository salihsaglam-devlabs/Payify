using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class BankLimitConfiguration : IEntityTypeConfiguration<BankLimit>
{
    public void Configure(EntityTypeBuilder<BankLimit> builder)
    {
        builder.Property(b => b.AcquireBankId).IsRequired();
        builder.Property(b => b.BankLimitType).IsRequired();
        builder.Property(b => b.LastValidDate).IsRequired();
        builder.Property(b => b.MarginRatio).IsRequired();
        builder.Property(b => b.MonthlyLimitAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(b => b.TotalAmount).IsRequired().HasPrecision(18, 4);
    }
}
