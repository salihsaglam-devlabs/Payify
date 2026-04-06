using LinkPara.LogConsumers.Commons.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace LinkPara.LogConsumers.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(t => t.SourceApplication).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Resource).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Operation).HasMaxLength(100).IsRequired();
        builder.Property(t => t.UserName).HasMaxLength(100);
        builder.Property(b => b.ClientIpAddress).HasMaxLength(50);
        builder.Property(b => b.Channel).HasMaxLength(150);
        builder.Property(b => b.Details).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v)
        );
        builder.Property(b => b.CorrelationId).HasMaxLength(100);
    }
}
