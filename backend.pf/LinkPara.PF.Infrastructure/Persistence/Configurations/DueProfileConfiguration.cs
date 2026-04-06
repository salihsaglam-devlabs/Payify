using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class DueProfileConfiguration : IEntityTypeConfiguration<DueProfile>
{
    public void Configure(EntityTypeBuilder<DueProfile> builder)
    {
        builder.Property(b => b.Title).IsRequired().HasMaxLength(250);
        builder.Property(b => b.OccurenceInterval).IsRequired();
        builder.Property(b => b.DueType).IsRequired();
        builder.Property(b => b.Amount).HasPrecision(18, 4);
        builder.Property(b => b.Currency).IsRequired();
        builder.Property(b => b.IsDefault).IsRequired();
    }
}