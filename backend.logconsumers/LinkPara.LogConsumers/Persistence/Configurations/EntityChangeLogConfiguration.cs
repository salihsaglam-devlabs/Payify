using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using LinkPara.LogConsumers.Commons.Entities;
using Newtonsoft.Json;

namespace LinkPara.LogConsumers.Persistence.Configurations
{
    public class EntityChangeLogConfiguration : IEntityTypeConfiguration<EntityChangeLog>
    {
        public void Configure(EntityTypeBuilder<EntityChangeLog> builder)
        {
            builder.Property(t => t.SchemaName).HasMaxLength(100).IsRequired();
            builder.Property(t => t.UserId).HasMaxLength(36).IsRequired();
            builder.Property(b => b.CrudOperationType).IsRequired();
            builder.Property(b => b.ClientIpAddress).HasMaxLength(50);
            builder.Property(b => b.ServiceName).HasMaxLength(150);

            builder.Property(b => b.NewValues).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v)
            );

            builder.Property(b => b.OldValues).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v)
            );

            builder.Property(b => b.KeyValues).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v)
            );

            builder.Property(b => b.AffectedColumns).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<List<string>>(v)
            );

            builder.Property(b=> b.CorrelationId).HasMaxLength(100);
        }
    }
}

