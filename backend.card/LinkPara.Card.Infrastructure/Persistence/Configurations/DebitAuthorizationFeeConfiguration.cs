using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using LinkPara.Card.Domain.Entities;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations
{
    public class DebitAuthorizationFeeConfiguration : IEntityTypeConfiguration<DebitAuthorizationFee>
    {
        public void Configure(EntityTypeBuilder<DebitAuthorizationFee> builder)
        {
            builder.Property(x => x.OceanTxnGUID).IsRequired();
        }
    }

}
