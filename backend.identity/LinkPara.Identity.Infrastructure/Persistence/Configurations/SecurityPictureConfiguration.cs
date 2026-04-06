using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;

public class SecurityPictureConfiguration : IEntityTypeConfiguration<SecurityPicture>
{
    public void Configure(EntityTypeBuilder<SecurityPicture> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        builder.Property(t => t.ContentType).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Bytes).IsRequired();
    }
}
