using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class AcquireBankConfiguration : IEntityTypeConfiguration<AcquireBank>
{
    public void Configure(EntityTypeBuilder<AcquireBank> builder)
    {
        builder.Property(b => b.PaymentGwTaxNo).HasMaxLength(11);
        builder.Property(b => b.PaymentGwTradeName).HasMaxLength(150);
        builder.Property(b => b.PaymentGwUrl).HasMaxLength(150);

        builder
            .HasOne(s => s.Bank)
            .WithMany()
            .HasForeignKey(s => s.BankCode)
            .HasPrincipalKey(s => s.Code)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
