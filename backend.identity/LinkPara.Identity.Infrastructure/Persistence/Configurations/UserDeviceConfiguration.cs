using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations
{
    public class UserDeviceConfiguration : IEntityTypeConfiguration<UserDeviceInfo>
    {
        public void Configure(EntityTypeBuilder<UserDeviceInfo> builder)
        {
            builder.HasOne(u => u.DeviceInfo);
        }
    }
}