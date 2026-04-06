using LinkPara.IKS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.IKS.Infrastructure.Persistence.Configurations;

public class IksTerminalConfiguration : IEntityTypeConfiguration<IksTerminal>
{
    public void Configure(EntityTypeBuilder<IksTerminal> builder)
    {
        builder.Property(s => s.MerchantId).IsRequired();
        builder.Property(s => s.ReferenceCode).IsRequired().HasMaxLength(12);
        builder.Property(s => s.GlobalMerchantId).IsRequired().HasMaxLength(8);
        builder.Property(s => s.PspMerchantId).IsRequired().HasMaxLength(15);
        builder.Property(s => s.TerminalId).HasMaxLength(10);
        builder.Property(s => s.StatusCode).IsRequired().HasMaxLength(1);
        builder.Property(s => s.Type).IsRequired().HasMaxLength(1);
        builder.Property(s => s.BrandCode).HasMaxLength(1);
        builder.Property(s => s.Model).HasMaxLength(10);
        builder.Property(s => s.SerialNo).HasMaxLength(25);
        builder.Property(s => s.OwnerPspNo).IsRequired();
        builder.Property(s => s.OwnerTerminalId).HasMaxLength(10);
        builder.Property(s => s.BrandSharing).HasMaxLength(1);
        builder.Property(s => s.PinPad).HasMaxLength(1);
        builder.Property(s => s.Contactless).HasMaxLength(1);
        builder.Property(s => s.ConnectionType).HasMaxLength(1);
        builder.Property(s => s.VirtualPosUrl).HasMaxLength(150);
        builder.Property(s => s.HostingTaxNo).HasMaxLength(11);
        builder.Property(s => s.HostingTradeName).HasMaxLength(150);
        builder.Property(s => s.HostingUrl).HasMaxLength(150);
        builder.Property(s => s.PaymentGwTaxNo).HasMaxLength(11);
        builder.Property(s => s.PaymentGwTradeName).HasMaxLength(150);
        builder.Property(s => s.PaymentGwUrl).HasMaxLength(150);
        builder.Property(s => s.ServiceProviderPspNo).IsRequired();
        builder.Property(s => s.FiscalNo).HasMaxLength(12);
        builder.Property(s => s.TechPos).IsRequired();
        builder.Property(s => s.ServiceProviderPspMerchantId).HasMaxLength(15);
        builder.Property(s => s.PfMainMerchantId).HasMaxLength(15);
        builder.Property(s => s.ResponseCode).HasMaxLength(3);
        builder.Property(s => s.ResponseCodeExplanation).HasMaxLength(2000);
    }
}