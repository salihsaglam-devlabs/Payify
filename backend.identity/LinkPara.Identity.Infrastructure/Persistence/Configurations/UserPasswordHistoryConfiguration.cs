using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations
{
    public class UserPasswordHistoryConfiguration : IEntityTypeConfiguration<UserPasswordHistory>
    {
        public void Configure(EntityTypeBuilder<UserPasswordHistory> builder)
        {

            builder.HasIndex(u => u.UserId);

        }
    }
}
