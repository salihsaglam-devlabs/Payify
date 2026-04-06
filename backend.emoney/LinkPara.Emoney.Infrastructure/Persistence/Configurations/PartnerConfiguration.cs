using LinkPara.Emoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Emoney.Infrastructure.Persistence.Configurations;

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(500);
        builder.Property(t => t.PartnerNumber).HasMaxLength(100);
        builder.Property(t => t.PhoneNumber).HasMaxLength(100);
        builder.Property(t => t.Email).HasMaxLength(300);

        builder.HasIndex(t => t.PartnerNumber);
    }
}