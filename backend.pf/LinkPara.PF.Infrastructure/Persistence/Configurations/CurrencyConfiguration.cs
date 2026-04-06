using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.HasIndex(t => t.Code).IsUnique();
        builder.Property(t => t.Code).IsRequired().HasMaxLength(10);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Symbol).IsRequired().HasMaxLength(5);
        builder.Property(t => t.Number).IsRequired().HasMaxLength(5);
    }
}
