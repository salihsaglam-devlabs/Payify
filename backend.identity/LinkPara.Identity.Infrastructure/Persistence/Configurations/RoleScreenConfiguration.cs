using LinkPara.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Infrastructure.Persistence.Configurations
{
    public class RoleScreenConfiguration : IEntityTypeConfiguration<RoleScreen>
    {
        public void Configure(EntityTypeBuilder<RoleScreen> builder)
        {

        }
    }
}