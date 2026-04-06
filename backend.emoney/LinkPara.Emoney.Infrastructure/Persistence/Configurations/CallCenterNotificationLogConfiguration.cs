using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class CallCenterNotificationLogConfiguration : IEntityTypeConfiguration<CallCenterNotificationLog>
{
    public void Configure(EntityTypeBuilder<CallCenterNotificationLog> builder)
    {
        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(b => b.LastName).IsRequired().HasMaxLength(50);
        builder.Property(b => b.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(b => b.ErrorMessage).HasMaxLength(300);
    }
}