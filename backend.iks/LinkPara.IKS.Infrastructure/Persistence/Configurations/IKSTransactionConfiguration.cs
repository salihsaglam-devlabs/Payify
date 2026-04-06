using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using LinkPara.IKS.Domain.Entities;
using Newtonsoft.Json;

namespace LinkPara.IKS.Infrastructure.Persistence.Configurations
{
    public class IKSTransactionConfiguration : IEntityTypeConfiguration<IKSTransaction>
    {
        public void Configure(EntityTypeBuilder<IKSTransaction> builder)
        {
            builder.Property(s => s.ResponseCode).HasMaxLength(20).IsRequired();
            builder.Property(s => s.MerchantId).IsRequired();
            builder.Property(s => s.Operation).IsRequired();

            builder.Property(b => b.RequestDetails).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v)
            );

            builder.Property(b => b.ResponseDetails).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v)
            );

        }
    }
}
