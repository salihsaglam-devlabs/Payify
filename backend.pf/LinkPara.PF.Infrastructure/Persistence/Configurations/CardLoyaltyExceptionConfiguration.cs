using LinkPara.PF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.PF.Infrastructure.Persistence.Configurations;

public class CardLoyaltyExceptionConfiguration : IEntityTypeConfiguration<CardLoyaltyException>
{
    public void Configure(EntityTypeBuilder<CardLoyaltyException> builder)
    {
        builder.Property(b => b.AllowOnUs).IsRequired();
        builder.Property(b => b.AllowInstallment).IsRequired();
        builder.Property(b => b.AllowPoint).IsRequired();
        
        builder
            .HasOne(s => s.Bank)
            .WithMany()
            .HasForeignKey(s => s.BankCode)
            .HasPrincipalKey(s => s.Code)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder
            .HasOne(s => s.CounterBank)
            .WithMany()
            .HasForeignKey(s => s.CounterBankCode)
            .HasPrincipalKey(s => s.Code)
            .OnDelete(DeleteBehavior.Restrict);
    }
}