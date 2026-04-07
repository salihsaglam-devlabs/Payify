using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using LinkPara.Card.Domain.Entities;

namespace LinkPara.Card.Infrastructure.Persistence.Configurations
{
    public class DebitAuthorizationConfiguration : IEntityTypeConfiguration<DebitAuthorization>
    {
        public void Configure(EntityTypeBuilder<DebitAuthorization> builder)
        {
            builder.Property(x => x.CorrelationID).IsRequired();
            builder.HasIndex(x => x.OceanTxnGUID);
        }
    }

}
