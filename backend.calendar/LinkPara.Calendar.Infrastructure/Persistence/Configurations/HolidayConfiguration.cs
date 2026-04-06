using LinkPara.Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Calendar.Infrastructure.Persistence.Configurations;

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.HolidayType).IsRequired().HasMaxLength(50);
        builder.Property(t => t.CountryCode).IsRequired().HasMaxLength(3);    
        builder.HasIndex(t => t.CountryCode);    
    }
}
