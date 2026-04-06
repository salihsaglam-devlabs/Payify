using LinkPara.Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Calendar.Infrastructure.Persistence.Configurations;

public class HolidayDetailConfiguration : IEntityTypeConfiguration<HolidayDetail>
{
    public void Configure(EntityTypeBuilder<HolidayDetail> builder)
    {
        builder.Property(t => t.BeginningTime).IsRequired();
        builder.Property(t => t.DurationInDays).IsRequired();
        builder.Property(t => t.EndingTime).IsRequired();
        builder.Property(t => t.DateOfHoliday).IsRequired();
        builder.HasIndex(t => t.HolidayId);

        builder.HasOne(s=>s.Holiday)
            .WithMany(s => s.HolidayDetails)
            .IsRequired()
            .HasForeignKey(s => s.HolidayId);
    }
}