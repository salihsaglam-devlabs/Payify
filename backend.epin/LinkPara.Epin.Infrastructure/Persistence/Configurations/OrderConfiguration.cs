using LinkPara.Epin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkPara.Epin.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(s => s.Pin).HasMaxLength(100);
        builder.Property(s => s.ProvisionReferenceId).HasMaxLength(100);
        builder.Property(s => s.WalletNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(s => s.ReferenceId).IsUnique();
        builder.Property(s => s.ReferenceId).HasMaxLength(50).IsRequired(); 
        builder.Property(s => s.UserId).IsRequired();
        builder.Property(t => t.Price).HasPrecision(18, 4);
        builder.Property(t => t.UnitPrice).HasPrecision(18, 4);
        builder.Property(t => t.Discount).HasPrecision(18, 4);
        builder.Property(t => t.Total).HasPrecision(18, 4);
        builder.Property(s => s.BrandId).IsRequired();
        builder.Property(s => s.PublisherId).IsRequired();
        builder.Property(s => s.Equivalent).HasMaxLength(300);
        builder.Property(s => s.UserFullName).HasMaxLength(250);
        builder.Property(s => s.PhoneNumber).HasMaxLength(50);
        builder.Property(s => s.ErrorMessage).HasMaxLength(250);
        builder.Property(s => s.Email).HasMaxLength(256);
    }
}