using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations
{
    public class PartnerCounterConfiguration : IEntityTypeConfiguration<PartnerCounter>
    {
        public void Configure(EntityTypeBuilder<PartnerCounter> builder)
        {
            builder.Property(t => t.Index).UseIdentityColumn();

            builder.HasIndex(t => t.Index);
        }
    }
}
