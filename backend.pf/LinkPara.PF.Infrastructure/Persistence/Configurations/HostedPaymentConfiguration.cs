using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class HostedPaymentConfiguration : IEntityTypeConfiguration<HostedPayment>
{
    public void Configure(EntityTypeBuilder<HostedPayment> builder)
    {
        builder.HasIndex(u => u.TrackingId).IsUnique();
        builder.HasIndex(u => u.ExpiryDate);
        builder.HasIndex(u => u.WebhookStatus);
        builder.Property(b => b.OrderId).IsRequired().HasMaxLength(24);
        builder.Property(b => b.Amount).HasPrecision(18, 4);
        builder.Property(b => b.CallbackUrl).IsRequired().HasMaxLength(250);
        builder.Property(b => b.ReturnUrl).HasMaxLength(250);
        builder.Property(b => b.Name).HasMaxLength(150);
        builder.Property(b => b.Surname).HasMaxLength(150);
        builder.Property(b => b.Email).IsRequired().HasMaxLength(256);
        builder.Property(b => b.PhoneNumber).HasMaxLength(10).IsRequired();
        builder.Property(b => b.ClientIpAddress).IsRequired().HasMaxLength(50);
        builder.Property(b => b.LanguageCode).IsRequired().HasMaxLength(2);
        builder.Property(b => b.MerchantName).IsRequired().HasMaxLength(150);
        builder.Property(b => b.MerchantNumber).IsRequired().HasMaxLength(15);
        builder.Property(b => b.ExpiryDate).IsRequired();
        builder.Property(b => b.TrackingId).IsRequired().HasMaxLength(24);
        builder.Property(b => b.PageViewType).IsRequired();
        builder.Property(b => b.EnableInstallments).IsRequired();
    }
}