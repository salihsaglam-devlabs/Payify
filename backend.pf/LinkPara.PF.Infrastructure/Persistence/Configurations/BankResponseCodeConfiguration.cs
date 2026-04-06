using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class BankResponseCodeConfiguration : IEntityTypeConfiguration<BankResponseCode>
{
    public void Configure(EntityTypeBuilder<BankResponseCode> builder)
    {
        builder.Property(b => b.ResponseCode).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Description).IsRequired().HasMaxLength(256);
        builder
              .HasOne(s => s.Bank)
              .WithMany()
              .HasForeignKey(s => s.BankCode)
              .HasPrincipalKey(s => s.Code)
              .OnDelete(DeleteBehavior.Restrict);
    }
}