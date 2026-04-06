using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using Newtonsoft.Json;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations
{
    public class DeviceInfoConfiguration : IEntityTypeConfiguration<DeviceInfo>
    {
        public void Configure(EntityTypeBuilder<DeviceInfo> builder)
        {
            builder.HasIndex(u => u.DeviceId);
            builder.Property(u => u.DeviceId).HasMaxLength(255);
            builder.Property(u => u.RegistrationToken).IsRequired();
            builder.Property(u => u.DeviceType).HasMaxLength(50);
            builder.Property(u => u.DeviceName).HasMaxLength(255);
            builder.Property(u => u.RegistrationToken).HasMaxLength(1000);
            builder.Property(u => u.Manufacturer).HasMaxLength(20);
            builder.Property(u => u.Model).HasMaxLength(255);
            builder.Property(u => u.OperatingSystem).HasMaxLength(50);
            builder.Property(u => u.OperatingSystemVersion).HasMaxLength(255);
            builder.Property(u => u.ScreenResolution).HasMaxLength(50);
            builder.Property(u => u.AppVersion).HasMaxLength(40);
            builder.Property(u => u.AppBuildNumber).HasMaxLength(255);
            builder.Property(u => u.Camera).HasMaxLength(1000);

        }
    }
}