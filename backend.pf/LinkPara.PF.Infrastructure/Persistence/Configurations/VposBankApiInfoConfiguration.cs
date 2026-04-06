using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class VposBankApiInfoConfiguration : IEntityTypeConfiguration<VposBankApiInfo>
{
    public void Configure(EntityTypeBuilder<VposBankApiInfo> builder)
    {
        builder.Property(b => b.Value).HasMaxLength(150);
        builder.Property(b => b.VposId).IsRequired();

        builder
           .HasOne(s => s.Key)
           .WithMany()
           .HasForeignKey(s => s.KeyId)
           .OnDelete(DeleteBehavior.Restrict);
    }
}
