using LinkPara.IKS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LinkPara.IKS.Infrastructure.Persistence.Configurations
{
    public class TimeoutIKSTransactionConfiguration : IEntityTypeConfiguration<TimeoutIKSTransaction>
    {
        public void Configure(EntityTypeBuilder<TimeoutIKSTransaction> builder)
        {
            builder.Property(s => s.MerchantId).IsRequired();
            builder.Property(s => s.ResponseCode).HasMaxLength(20).IsRequired();
            builder.Property(s => s.Operation).IsRequired();

            builder.Property(b => b.RequestDetails).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v)
            );

            builder.Property(b => b.ResponseDetails).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v)
            );

            builder.Property(b => b.TimeoutReturnDetails).HasConversion(
           v => JsonConvert.SerializeObject(v),
           v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v)
           );

        }
    }
}

