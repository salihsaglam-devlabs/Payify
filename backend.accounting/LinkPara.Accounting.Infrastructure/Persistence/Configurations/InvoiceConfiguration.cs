using LinkPara.Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Accounting.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.Property(s => s.Code).HasMaxLength(50);
        builder.Property(s => s.TotalCommission).HasPrecision(18, 4);
        builder.Property(s => s.TotalBsmv).HasPrecision(18, 4);
    }
}
