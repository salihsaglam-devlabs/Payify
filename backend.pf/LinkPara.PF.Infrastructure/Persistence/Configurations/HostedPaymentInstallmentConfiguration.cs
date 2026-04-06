using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class HostedPaymentInstallmentConfiguration : IEntityTypeConfiguration<HostedPaymentInstallment>
{
    public void Configure(EntityTypeBuilder<HostedPaymentInstallment> builder)
    {
        builder
            .HasOne(b => b.HostedPayment)
            .WithMany(b => b.Installments)
            .HasForeignKey(c => c.HostedPaymentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(b => b.Amount).HasPrecision(18, 4);
        builder.Property(b => b.CardNetwork).IsRequired();
    }
}