using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations;

public class UserSecurityPictureConfiguration : IEntityTypeConfiguration<UserSecurityPicture>
{
    public void Configure(EntityTypeBuilder<UserSecurityPicture> builder)
    {
        builder.HasIndex(u => u.UserId);
        builder.HasIndex(u => u.SecurityPictureId);
    }
}
