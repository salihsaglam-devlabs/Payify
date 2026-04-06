
using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.Property(s => s.PublicKey).HasMaxLength(200).IsRequired();
        builder.Property(s => s.PrivateKey).HasMaxLength(4001).IsRequired();
        builder.HasIndex(t => t.PublicKey);

        builder.HasOne(s => s.Partner)
                .WithMany()
                .HasForeignKey(s => s.PartnerId)
                .HasPrincipalKey(s => s.Id)
                .OnDelete(DeleteBehavior.Restrict);
    }
}
