using LinkPara.Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Accounting.Infrastructure.Persistence.Configurations;

public class ExternalCurrencyConfiguration : IEntityTypeConfiguration<ExternalCurrency>
{
    public void Configure(EntityTypeBuilder<ExternalCurrency> builder)
    {
        builder.HasIndex(s => new { s.Code, s.AccountingCustomerType }).IsUnique();
        builder.Property(s => s.Code).HasMaxLength(10);
        builder.Property(s => s.Name).HasMaxLength(100);
        builder.Property(s => s.AccountCode).HasMaxLength(100);
    }
}
