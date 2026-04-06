using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations
{
    public class UserPictureConfiguration : IEntityTypeConfiguration<UserPicture>
    {
        public void Configure(EntityTypeBuilder<UserPicture> builder)
        {

            builder.HasIndex(u => u.UserId);

        }
    }
}